import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject } from '@angular/core';
import { DataSource } from '@angular/cdk/collections';
import { MdPaginator, PageEvent, MdSort, MdDialog, MdDialogRef, MD_DIALOG_DATA, MdSnackBar } from '@angular/material';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subject } from 'rxjs/Subject';
import { Subscription } from 'rxjs/Subscription';

import { TvgMediaService } from '../../services/tvgmedia/tvgmedia.service';
import { CommonService, Constants } from '../../services/common/common.service';
import { Observable } from "rxjs/Observable";
import { TvgMedia, ElasticQuery } from "../../types/elasticQuery.type";
import 'rxjs/add/observable/fromEvent';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/observable/merge';
import { EventTargetLike } from "rxjs/observable/FromEventObservable";

@Component({
    selector: 'app-media',
    templateUrl: './media.component.html',
    providers: [TvgMediaService, CommonService]
})
export class MediaComponent implements OnInit, OnDestroy {
    subscriptionTableEvent: Subscription;

    displayedColumns = ['position', 'name', 'group', 'lang', 'actions'];
    @ViewChild(MdPaginator) paginator: MdPaginator;
    @ViewChild(MdSort) sort: MdSort;
    @ViewChild('filter') filter: ElementRef;
    dataSource: MediaDataSource | null;
    currentItem: TvgMedia | null;

    /** media ctor */
    constructor(private tvgMediaService: TvgMediaService, private commonService: CommonService, public dialog: MdDialog, public snackBar: MdSnackBar) { }

    /** Called by Angular after media component initialized */
    ngOnInit(): void {
        let storedQuery = this.commonService.JsonToObject<ElasticQuery>(localStorage.getItem(Constants.LS_MediaQueryKey));
        console.log("storedQuery ", storedQuery);

        this.paginator.pageIndex = storedQuery != null ? Math.floor(storedQuery.from / storedQuery.size) : 0;
        this.paginator.pageSizeOptions = [50, 100, 250, 1000];
        this.paginator.pageSize = storedQuery != null ? storedQuery.size : this.paginator.pageSizeOptions[0];
        this.filter.nativeElement.value = storedQuery != null && storedQuery.query != null && storedQuery.query != {} ? JSON.stringify(storedQuery.query) : "";
        storedQuery = null;

        this.dataSource = new MediaDataSource(this.tvgMediaService, this.paginator, this.sort);

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

    openDialog(media: TvgMedia): void {
        let dialogRef = this.dialog.open(TvgMediaModifyDialog, {
            width: '550px',
            height: '500px',
            data: media
        });

        dialogRef.afterClosed().subscribe(result => {
            this.snackBar.open(media.name + " was modified", "", { duration: 400 });
        });
    }

    update(media: TvgMedia): void {
        console.log(media);
    }

    ngOnDestroy() {
        this.subscriptionTableEvent.unsubscribe();
    }
}

@Component({
    selector: 'media-modify-dialog',
    templateUrl: './media.dialog.html'
})
export class TvgMediaModifyDialog {

    constructor(
        public dialogRef: MdDialogRef<TvgMediaModifyDialog>,
        @Inject(MD_DIALOG_DATA) public data: any) { }

    onNoClick(): void {
        this.dialogRef.close();
    }

}

/**
 * Media datasource for md-table component
 */
export class MediaDataSource extends DataSource<TvgMedia> {

    resultsLength = 0;
    isLoadingResults = false;

    medias = new BehaviorSubject<TvgMedia[]>([]);

    _filterChange = new BehaviorSubject<Object | string>({});
    get filter(): Object | string { return this._filterChange.value; }
    set filter(filter: Object | string) { this._filterChange.next(filter); }

    _paginator = new BehaviorSubject<MdPaginator>(<MdPaginator>{});
    get paginator(): MdPaginator { return this._paginator.value; }
    set paginator(paginator: MdPaginator) { this._paginator.next(paginator); }

    constructor(private tvgMediaService: TvgMediaService, private mdPaginator: MdPaginator, private _sort: MdSort) {
        super();

        this.paginator = mdPaginator;
        this._filterChange
            .merge(this._paginator)
            .debounceTime(300)
            //.distinctUntilChanged()
            .subscribe((x) => this.getData());
    }

    connect(): Observable<TvgMedia[]> {
        const displayDataChanges = [
            this.medias,
            this._sort.sortChange
        ];

        return Observable.merge(...displayDataChanges).map(() => {
            return this.medias.value;
        });
    }
    /**
     * Get medias list from webapi
     * @returns Obersvable<TvgMedia[]>
     */
    getData(): Observable<TvgMedia[]> {
        this.isLoadingResults = true;
        let pageSize = this.paginator.pageSize === undefined ? 25 : this.paginator.pageSize;
        let query = <ElasticQuery>{
            from: pageSize * this.paginator.pageIndex,
            size: pageSize
        };
        if (typeof this.filter === "string") {
            if (this.filter !== undefined && this.filter != "")
                query.query = {
                    match: { "name": this.filter }
                };
        }
        else {
            query.query = this.filter;
        }
        localStorage.setItem(Constants.LS_MediaQueryKey, JSON.stringify(query));
        let res = this.tvgMediaService.list(query).map((v, i) => {
            console.log("recup medias ", v);
            this._paginator.value.length = v.total;
            this.isLoadingResults = false;
            return v.result;
        });
        res.subscribe(x => { this.medias.next(x); });
        return res;
    }

    disconnect() { }
}

