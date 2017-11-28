import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject } from '@angular/core';
import { DataSource } from '@angular/cdk/collections';
import { MatPaginator, PageEvent, MatSort, MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subject } from 'rxjs/Subject';
import { Subscription } from 'rxjs/Subscription';

import { CommonService, Constants } from '../../services/common/common.service';
import { Observable } from "rxjs/Observable";
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import 'rxjs/add/observable/fromEvent';
import { distinctUntilChanged, merge, debounceTime } from 'rxjs/operators';
import { EventTargetLike } from "rxjs/observable/FromEventObservable";
import { MediaRefService } from '../../services/mediaref/mediaref.service';
import { mediaRef } from '../../types/mediaref.type';
import { PiconService } from '../../services/picons/picons.service';
import { picon } from '../../types/picon.type';

//{"match":{"groups":"bein.net"}}
//{"match":{"cultures":"International"}}

@Component({
    selector: 'mediaref',
    templateUrl: './mediaref.component.html',
    providers: [MediaRefService, CommonService]
})
/** mediaref component*/
export class MediaRefComponent implements OnInit, OnDestroy {
    subscriptionTableEvent: Subscription;

    displayedColumns = ['logo', 'displayNames', 'groups', 'cultures', 'mediaType', 'actions'];
    @ViewChild(MatPaginator) paginator: MatPaginator;
    @ViewChild(MatSort) sort: MatSort;
    @ViewChild('filter') filter: ElementRef;
    dataSource: MediaRefDataSource | null;
    currentItem: mediaRef | null;

    /** media ctor */
    constructor(private mediaRefService: MediaRefService, private piconService: PiconService, private commonService: CommonService, public dialog: MatDialog, public snackBar: MatSnackBar) { }

    /** Called by Angular after media component initialized */
    ngOnInit(): void {
        let storedQuery = this.commonService.JsonToObject<ElasticQuery>(localStorage.getItem(Constants.LS_MediaRefQueryKey));
        console.log("storedQuery ", storedQuery);

        this.paginator.pageIndex = storedQuery != null ? Math.floor(storedQuery.from / storedQuery.size) : 0;
        this.paginator.pageSizeOptions = [50, 100, 250, 1000];
        this.paginator.pageSize = storedQuery != null ? storedQuery.size : this.paginator.pageSizeOptions[0];
        this.filter.nativeElement.value = storedQuery != null && storedQuery.query != null && storedQuery.query != {} ? JSON.stringify(storedQuery.query) : "";
        storedQuery = null;

        this.dataSource = new MediaRefDataSource(this.mediaRefService, this.paginator);

        this.subscriptionTableEvent = this.paginator.page.asObservable()
            .merge(Observable.fromEvent<EventTargetLike>(this.filter.nativeElement, 'keyup'))
            .debounceTime(1000)
            .distinctUntilChanged()
            .subscribe((x) => {
                if (!this.dataSource) { return; }
                console.log("subscriptionTableEvent => ", x);
                if ((x as PageEvent).length === undefined)
                    this.paginator.pageIndex = 0;

                let objectQuery = this.commonService.JsonToObject<any>(this.filter.nativeElement.value);
                console.log('objectQuery => ', objectQuery, this.filter.nativeElement.value);
                this.dataSource.filter = objectQuery != null ? objectQuery : this.filter.nativeElement.value;
                this.dataSource.paginator = this.paginator;
            });
    }

    openDialog(spChannel: mediaRef): void {
        let dialogRef = this.dialog.open(MediaRefModifyDialog, {
            width: '550px',
            height: '500px',
            data: spChannel
        });

        dialogRef.afterClosed().subscribe(result => {
            this.snackBar.open(spChannel.displayNames[0] + " was modified", "", { duration: 400 });
        });
    }

    update(spChannel: mediaRef): void {

    }

    delete(id: string): void {
        this.dataSource.delete(id);
        /**
         * .subscribe(res => {
                this.snackBar.open("media ref removed");
            });
         */
    }

    synk(): void {
        this.mediaRefService.synk().subscribe(res => {
            this.snackBar.open("Medias referentiel was synchronized");
        });
    }

    save(): void {
        console.log("saving mediasref..");
        this.dataSource.save().subscribe(res => {
            this.snackBar.open("Medias was saved successfully");
        });
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

@Component({
    selector: 'mediaref-modify-dialog',
    templateUrl: './mediaref.dialog.html'
})
export class MediaRefModifyDialog implements OnInit, OnDestroy {

    @ViewChild('filterPicon') filterPicon: ElementRef;
    piconsFilter: Observable<picon[]>;

    constructor(
        private piconService: PiconService,
        public dialogRef: MatDialogRef<MediaRefModifyDialog>,
        @Inject(MAT_DIALOG_DATA) public data: any) {
    }

    ngOnInit(): void {
        Observable.fromEvent<EventTargetLike>(this.filterPicon.nativeElement, 'keyup')
            .debounceTime(1000)
            .distinctUntilChanged()
            .subscribe((x) => {
                this.piconsFilter = this.piconService.list("name", this.filterPicon.nativeElement.value).map(x => x.result);
            });
    }

    onNoClick(): void {
        this.dialogRef.close();
    }

    parseListText(model: any) {
        model = model.split("\n");
    }

    //autoCompletePicons(filter: string): Array<picon>{

    //}

    ngOnDestroy(): void {
    }

}

/**
 * Media datasource for mat-table component
 */
export class MediaRefDataSource extends DataSource<mediaRef> {

    data: mediaRef[];
    medias = new BehaviorSubject<mediaRef[]>([]);
    _filterChange = new BehaviorSubject<Object | string>({});
    get filter(): Object | string { return this._filterChange.value; }
    set filter(filter: Object | string) { this._filterChange.next(filter); }

    _paginator = new BehaviorSubject<MatPaginator>(<MatPaginator>{});
    get paginator(): MatPaginator { return this._paginator.value; }
    set paginator(paginator: MatPaginator) { this._paginator.next(paginator); }

    constructor(private mediaRefService: MediaRefService, private mdPaginator: MatPaginator) {
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
        let query = <ElasticQuery>{
            from: pageSize * this.paginator.pageIndex,
            size: pageSize
        };

        if (typeof this.filter === "string") {
            if (this.filter !== undefined && this.filter != "")
                query.query = {
                    match: { "_all": this.filter }
                };
        }
        else {
            query.query = this.filter;
        }

        localStorage.setItem(Constants.LS_MediaQueryKey, JSON.stringify(query));
        let res = this.mediaRefService.list(query).map((v, i) => {
            console.log("recup epg ", v);
            this._paginator.value.length = v.total;
            return v.result;
        });
        res.subscribe(x => {
            this.medias.next(x);
            this.data = [];
            this.data.concat(x);
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
        return this.mediaRefService.save(this.medias.value)
    }

    disconnect() { }
}

