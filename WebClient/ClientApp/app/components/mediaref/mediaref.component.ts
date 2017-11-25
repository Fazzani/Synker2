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

@Component({
    selector: 'mediaref',
    templateUrl: './mediaref.component.html',
    providers: [MediaRefService, CommonService]
})
/** mediaref component*/
export class MediaRefComponent implements OnInit, OnDestroy {
    subscriptionTableEvent: Subscription;

    displayedColumns = ['logo', 'displayNames', 'groups', 'cultures', 'mediaType'];
    @ViewChild(MatPaginator) paginator: MatPaginator;
    @ViewChild(MatSort) sort: MatSort;
    @ViewChild('filter') filter: ElementRef;
    dataSource: MediaRefDataSource | null;
    currentItem: mediaRef | null;

    /** media ctor */
    constructor(private mediaRefService: MediaRefService, private commonService: CommonService, public dialog: MatDialog, public snackBar: MatSnackBar) { }

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

                let objectQuery = this.commonService.JsonToObject(this.filter.nativeElement.value);
                console.log('objectQuery => ', objectQuery);
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

    ngOnDestroy() {
        this.subscriptionTableEvent.unsubscribe();
    }
}

@Component({
    selector: 'mediaref-modify-dialog',
    templateUrl: './mediaref.dialog.html'
})
export class MediaRefModifyDialog {

    constructor(
        public dialogRef: MatDialogRef<MediaRefModifyDialog>,
        @Inject(MAT_DIALOG_DATA) public data: any) { }

    onNoClick(): void {
        this.dialogRef.close();
    }
}

/**
 * Media datasource for mat-table component
 */
export class MediaRefDataSource extends DataSource<mediaRef> {

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

    connect(): Observable<mediaRef[]> { return this.medias.asObservable() };

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
                    match: { "_all" : this.filter }
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
        res.subscribe(x => { this.medias.next(x); });
        return res;
    }

    save(): any {
        return this.mediaRefService.save(this.medias.value);
    }

    disconnect() { }
}

