import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject } from '@angular/core';
import { DataSource } from '@angular/cdk/collections';
import { MatPaginator, PageEvent, MatSort, MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subject } from 'rxjs/Subject';
import { Subscription } from 'rxjs/Subscription';

import { XmltvService } from '../../services/xmltv/xmltv.service';
import { CommonService, Constants } from '../../services/common/common.service';
import { Observable } from "rxjs/Observable";
import { ElasticQuery } from "../../types/elasticQuery.type";
import 'rxjs/add/observable/fromEvent';
import { distinctUntilChanged, merge, debounceTime } from 'rxjs/operators';
import { EventTargetLike } from "rxjs/observable/FromEventObservable";
import { sitePackChannel } from '../../types/sitepackchannel.type';

@Component({
    selector: 'xmltv',
    templateUrl: './xmltv.component.html',
    providers: [XmltvService, CommonService]
})
/** media component*/
export class XmltvComponent implements OnInit, OnDestroy {
    subscriptionTableEvent: Subscription;

    displayedColumns = ['site_id', 'site', 'xmltv_id', 'channel_name'];
    @ViewChild(MatPaginator) paginator: MatPaginator;
    @ViewChild(MatSort) sort: MatSort;
    @ViewChild('filter') filter: ElementRef;
    dataSource: XmltvDataSource | null;
    currentItem: sitePackChannel | null;

    /** media ctor */
    constructor(private xmltvService: XmltvService, private commonService: CommonService, public dialog: MatDialog, public snackBar: MatSnackBar) { }

    /** Called by Angular after media component initialized */
    ngOnInit(): void {
        let storedQuery = this.commonService.JsonToObject<ElasticQuery>(localStorage.getItem(Constants.LS_MediaQueryKey));
        console.log("storedQuery ", storedQuery);

        this.paginator.pageIndex = storedQuery != null ? Math.floor(storedQuery.from / storedQuery.size) : 0;
        this.paginator.pageSizeOptions = [50, 100, 250, 1000];
        this.paginator.pageSize = storedQuery != null ? storedQuery.size : this.paginator.pageSizeOptions[0];
        this.filter.nativeElement.value = storedQuery != null && storedQuery.query != null && storedQuery.query != {} ? JSON.stringify(storedQuery.query) : "";
        storedQuery = null;

        this.dataSource = new XmltvDataSource(this.xmltvService, this.paginator);

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

    openDialog(spChannel: sitePackChannel): void {
        let dialogRef = this.dialog.open(XmltvModifyDialog, {
            width: '550px',
            height: '500px',
            data: spChannel
        });

        dialogRef.afterClosed().subscribe(result => {
            this.snackBar.open(spChannel.channel_name + " was modified", "", { duration: 400 });
        });
    }

    update(spChannel: sitePackChannel): void {
        console.log(spChannel);
    }

    ngOnDestroy() {
        this.subscriptionTableEvent.unsubscribe();
    }
}

@Component({
    selector: 'xmltv-modify-dialog',
    templateUrl: './xmltv.dialog.html'
})
export class XmltvModifyDialog {

    constructor(
        public dialogRef: MatDialogRef<XmltvModifyDialog>,
        @Inject(MAT_DIALOG_DATA) public data: any) { }

    onNoClick(): void {
        this.dialogRef.close();
    }

}

/**
 * Media datasource for mat-table component
 */
export class XmltvDataSource extends DataSource<sitePackChannel> {

    medias = new BehaviorSubject<sitePackChannel[]>([]);

    _filterChange = new BehaviorSubject<Object | string>({});
    get filter(): Object | string { return this._filterChange.value; }
    set filter(filter: Object | string) { this._filterChange.next(filter); }

    _paginator = new BehaviorSubject<MatPaginator>(<MatPaginator>{});
    get paginator(): MatPaginator { return this._paginator.value; }
    set paginator(paginator: MatPaginator) { this._paginator.next(paginator); }

    constructor(private xmltvService: XmltvService, private mdPaginator: MatPaginator) {
        super();

        this.paginator = mdPaginator;
        this._filterChange
            .merge(this._paginator)
            .debounceTime(300)
            //.distinctUntilChanged()
            .subscribe((x) => this.getData());
    }

    connect(): Observable<sitePackChannel[]> { return this.medias.asObservable() };

    /**
     * Get medias list from webapi
     * @returns Obersvable<tvChannel[]>
     */
    getData(): Observable<sitePackChannel[]> {
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
        let res = this.xmltvService.listSitePack(query).map((v, i) => {
            console.log("recup epg ", v);
            this._paginator.value.length = v.total;
            return v.result;
        });
        res.subscribe(x => { this.medias.next(x); });
        return res;
    }

    disconnect() { }
}

