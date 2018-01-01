import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject } from '@angular/core';
import { DataSource } from '@angular/cdk/collections';
import { MatPaginator, PageEvent, MatSort, MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subject } from 'rxjs/Subject';
import { Subscription } from 'rxjs/Subscription';
import { ENTER, COMMA } from '@angular/cdk/keycodes';
import { MatChipInputEvent } from '@angular/material';

import { CommonService, Constants } from '../../services/common/common.service';
import { Observable } from "rxjs/Observable";
import { ElasticQuery, ElasticResponse, SimpleQueryElastic } from "../../types/elasticQuery.type";
import 'rxjs/add/observable/fromEvent';
import { distinctUntilChanged, merge, debounceTime } from 'rxjs/operators';
import { EventTargetLike } from "rxjs/observable/FromEventObservable";
import { MediaRefService } from '../../services/mediaref/mediaref.service';
import { mediaRef } from '../../types/mediaref.type';
import { PiconService } from '../../services/picons/picons.service';
import { picon } from '../../types/picon.type';
import { FormControl } from "@angular/forms";
import { startWith } from "rxjs/operator/startWith";
import { map } from "rxjs/operator/map";
import { snakbar_duration } from '../../variables';

@Component({
    selector: 'mediaref',
    templateUrl: './mediaref.component.html',
    providers: [MediaRefService]
})
/** mediaref component*/
export class MediaRefComponent implements OnInit, OnDestroy {
    subscriptionTableEvent: Subscription;

    displayedColumns = ['logo', 'displayNames', 'groups', 'cultures', 'defaultSite', 'mediaType', 'actions'];
    @ViewChild(MatPaginator) paginator: MatPaginator;
    @ViewChild(MatSort) sort: MatSort;
    @ViewChild('filter') filter: ElementRef;
    dataSource: MediaRefDataSource | null;
    currentItem: mediaRef | null;

    /** media ctor */
    constructor(private mediaRefService: MediaRefService, private piconService: PiconService, private commonService: CommonService, public dialog: MatDialog, public snackBar: MatSnackBar) { }

    /** Called by Angular after media component initialized */
    ngOnInit(): void {
        let storedQuery = this.commonService.JsonToObject<SimpleQueryElastic>(localStorage.getItem(Constants.LS_MediaRefQueryKey));
        console.log("storedQuery ", storedQuery);

        this.paginator.pageIndex = storedQuery != null ? Math.floor(storedQuery.From / storedQuery.Size) : 0;
        this.paginator.pageSizeOptions = [50, 100, 250, 1000];
        this.paginator.pageSize = storedQuery != null ? storedQuery.Size : this.paginator.pageSizeOptions[0];
        this.dataSource = new MediaRefDataSource(this.commonService, this.mediaRefService, this.paginator);
        this.dataSource.filter = this.filter.nativeElement.value = storedQuery.Query;

        this.subscriptionTableEvent = this.paginator.page.asObservable()
            .merge(Observable.fromEvent<EventTargetLike>(this.filter.nativeElement, 'keyup'))
            .debounceTime(1500)
            .distinctUntilChanged()
            .subscribe((x) => {
                if (!this.dataSource) { return; }
                console.log("subscriptionTableEvent => ", x);
                if ((x as PageEvent).length === undefined)
                    this.paginator.pageIndex = 0;

                this.dataSource.filter = this.filter.nativeElement.value;
                this.dataSource.paginator = this.paginator;
            });

    }

    /**
     * Modify mediaRef dialog
     * @param spChannel
     */
    openDialog(spChannel: mediaRef): void {
        let dialogRef = this.dialog.open(MediaRefModifyDialog, {
            width: '550px',
            data: spChannel
        });

        dialogRef.afterClosed().subscribe(result => {
            this.snackBar.open(spChannel.displayNames[0] + " was modified", "", { duration: snakbar_duration });
        });
    }

    /**
     * Synchronize mediaRef from sitepack index
     */
    synk(): void {
        this.mediaRefService.synk().subscribe(res => {
            this.snackBar.open("Medias referentiel was synchronized");
        });
    }

    /**
     * Synchronize all picons from github
     */
    synkPiconsGlobal(): void {
        this.piconService.synk().subscribe(res => {
            this.snackBar.open("Picons index was synchronized");
        });
    }

    /**
     * Match mediaRef with picons
     */
    synkPiconsForMediaRef(): void {
        this.mediaRefService.synkPicons().subscribe(res => {
            this.snackBar.open("Picons was synchronized for all mediaRef");
            this.reload();
        });
    }

    delete(id: string): void {
        const confirm = window.confirm('Do you really want to delete this media ref?');

        Observable
            .of(id)
            .filter(() => confirm)
            .switchMap(x => this.mediaRefService.delete(x))
            .subscribe(res => this.snackBar.open("Medias referentiel was deleted"));
    }

    save(): void {
        console.log("saving mediasref..");
        this.dataSource.save().subscribe(res => {
            this.snackBar.open("Medias was saved successfully");
        });
    }

    update(spChannel: mediaRef): void {
        this.mediaRefService.save(spChannel).subscribe(x =>
            this.snackBar.open("Medias referentiel was synchronized")
        );
    }

    reload(): void {
        this.ngOnInit();
    }

    toggleSelected(media: mediaRef, event: any): void {
        if (!event.ctrlKey) {
            this.dataSource.data.filter((v, i) => v.id != media.id).forEach((m, i) => {
                m.selected = false;
            });
        }
        media.selected = !media.selected;
    }

    ngOnDestroy() {
        this.subscriptionTableEvent.unsubscribe();
    }
}

//---------------------------------------------------------------------------------   MediaRef ModifyDialog
@Component({
    selector: 'mediaref-modify-dialog',
    templateUrl: './mediaref.dialog.html'
})
export class MediaRefModifyDialog implements OnInit, OnDestroy {

    @ViewChild('filterPicon') filterPicon: ElementRef;
    piconsFilter: Observable<picon[]>;
    currrentPiconUrl: string;
    visible: boolean = true;
    selectable: boolean = true;
    removable: boolean = true;
    addOnBlur: boolean = true;

    // Enter, comma
    separatorKeysCodes = [ENTER, COMMA];
    constructor(
        private piconService: PiconService,
        public dialogRef: MatDialogRef<MediaRefModifyDialog>,
        @Inject(MAT_DIALOG_DATA) public data: any) {
    }

    ngOnInit(): void {
        Observable.fromEvent<EventTargetLike>(this.filterPicon.nativeElement, 'keyup')
            .debounceTime(1000)
            .distinctUntilChanged()
            .subscribe((x: EventTargetLike) => {
                let query = <SimpleQueryElastic>{
                    From: 0, IndexName: 'mediaref', Query: `name: ${this.filterPicon.nativeElement.value}`, Size: 10
                }
                this.piconsFilter = this.piconService.search(query).map(x => x.result).do(x => console.log(x));
            });
    }

    onNoClick(): void {
        this.dialogRef.close();
    }

    onfocusPicon(piconUrl: string): void {
        this.currrentPiconUrl = piconUrl;
        this.filterPicon.nativeElement.value = "";
    }

    onblurPicon(piconPath: string): void {
        if (piconPath == '')
            this.filterPicon.nativeElement.value = this.currrentPiconUrl;
    }

    add(event: MatChipInputEvent, model): void {
        let input = event.input;
        let value = event.value;
        if ((value || '').trim()) {
            model.push(value.trim());
        }

        // Reset the input value
        if (input) {
            input.value = '';
        }
    }

    remove(data: any, model: any): void {
        let index = model.indexOf(data);

        if (index >= 0) {
            model.splice(index, 1);
        }
    }

    ngOnDestroy(): void {
    }

}

//---------------------------------------------------------------------------------   MediaRef DataSource
/**
 * Media datasource for mat-table component
 */
export class MediaRefDataSource extends DataSource<mediaRef> {

    data: mediaRef[];
    medias = new BehaviorSubject<mediaRef[]>([]);
    _filterTvgSiteChange = new BehaviorSubject<string>('');

    _filterChange = new BehaviorSubject<string>('');
    get filter(): string { return this._filterChange.value; }
    set filter(filter: string) { this._filterChange.next(filter); }

    _paginator = new BehaviorSubject<MatPaginator>(<MatPaginator>{});
    get paginator(): MatPaginator { return this._paginator.value; }
    set paginator(paginator: MatPaginator) { this._paginator.next(paginator); }

    constructor(private commonService: CommonService, private mediaRefService: MediaRefService, private mdPaginator: MatPaginator) {
        super();

        this.paginator = mdPaginator;
        this._filterChange
            .merge(this._paginator)
            .debounceTime(300)
            //.distinctUntilChanged()
            .subscribe((x) => this.getData());
    }

    connect(): Observable<mediaRef[]> {
        return this.medias.asObservable();
    };

    /**
     * Get mediasref list from webapi
     * @returns Obersvable<mediaRef[]>
     */
    getData(): Observable<mediaRef[]> {
        let pageSize = this.paginator.pageSize === undefined ? 25 : this.paginator.pageSize;
        let query = <SimpleQueryElastic>{ From: pageSize * (isNaN(this.paginator.pageIndex) ? 0 : this.paginator.pageIndex), IndexName: 'mediaref', Query: this.filter, Size: pageSize }

        localStorage.setItem(Constants.LS_MediaRefQueryKey, JSON.stringify(query));

        let res = this.mediaRefService.search<mediaRef>(query).map((v, i) => {
            console.log("recup epg ", v);
            this._paginator.value.length = v.total;
            return v.result;
        });

        res.subscribe(x => {
            this.medias.next(x);
            this.data = x;
        });

        return res;
    }

    delete(id: string): void {
        this.mediaRefService.delete(id).subscribe((res: number) => {
            var idx = this.medias.value.findIndex(m => m.id == id);
            this.medias.value.splice(idx, 1);
            this.medias.next(this.medias.value);
        });
    }

    save(): any {
        return this.mediaRefService.save(...this.medias.value)
    }

    disconnect() { }
}

