import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject } from '@angular/core';
import { DataSource } from '@angular/cdk/collections';
import { MdPaginator, PageEvent, MdSort, MdDialog, MdDialogRef, MD_DIALOG_DATA, MdSnackBar } from '@angular/material';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subject } from 'rxjs/Subject';
import { Subscription } from 'rxjs/Subscription';

import { TvgMediaService } from '../../services/tvgmedia/tvgmedia.service';
import { CommonService, Constants } from '../../services/common/common.service';
import { Observable } from "rxjs/Observable";
import { TvgMedia, ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import 'rxjs/add/observable/fromEvent';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/observable/merge';
import 'rxjs/add/observable/merge';
import 'rxjs/add/observable/of';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/startWith';
import 'rxjs/add/operator/switchMap';
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

        this.subscriptionTableEvent = Observable.fromEvent<EventTargetLike>(this.filter.nativeElement, 'keyup')
            .debounceTime(1000)
            .distinctUntilChanged()
            .subscribe(x => {
                if (!this.dataSource) { return; }
                let objectQuery = this.commonService.JsonToObject(this.filter.nativeElement.value);
                console.log('objectQuery => ', objectQuery);
                this.paginator.pageIndex = 0;
                this.dataSource.filter = objectQuery != null ? objectQuery : this.filter.nativeElement.value;
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
    took: number;
    maxScore: number;
    hits: number;
    total: number;
    isLoadingResults = true;

    _filterChange = new BehaviorSubject<Object | string>({});
    get filter(): Object | string { return this._filterChange.value; }
    set filter(filter: Object | string) { this._filterChange.next(filter); }

    constructor(private tvgMediaService: TvgMediaService, private mdPaginator: MdPaginator, private _sort: MdSort) {
        super();

        // this._filterChange
        //     .merge(this.mdPaginator)
        //     .debounceTime(300)
        //     //.distinctUntilChanged()
        //     .subscribe((x) => this.getData());
    }

    connect(): Observable<TvgMedia[]> {
        const displayDataChanges = [
            this._sort.sortChange,
            this.mdPaginator.page,
            this.mdPaginator.pageSize
        ];

        return this._filterChange.merge(Observable.merge(...displayDataChanges))
            .startWith(null)
            .switchMap(() => {
                this.isLoadingResults = true;
                return this.getData();
            })
            .map(data => {
                console.dir(data);
                // Flip flag to show that loading has finished.
                this.isLoadingResults = false;
                this.hits = data.hits;
                this.maxScore = data.maxScore;
                this.took = data.took;
                this.total = data.total;

                return data.result;
            })
            .catch(() => {
                this.isLoadingResults = false;
                // Catch if the GitHub API has reached its rate limit. Return empty data.
                return Observable.of([]);
            });
    }
    /**
     * Get medias list from webapi
     * @returns Obersvable<TvgMedia[]>
     */
    getData(): Observable<ElasticResponse<TvgMedia>> {
        this.isLoadingResults = true;

        //TODO: à virer et la remplacer par un decorator
        let defaultObjectTvgMedia = <TvgMedia>{ position: 0, name: 'jj', group: 'sd', lang: 'sq' };
        let typeProp = typeof defaultObjectTvgMedia[this._sort.active];

        let sortField = this._sort.active;
        if (typeof defaultObjectTvgMedia[this._sort.active] === "string")
            sortField += ".keyword";

        let sortObject = JSON.parse('{"' + sortField + '" : {"order":"' + this._sort.direction + '"}}');

        let pageSize = this.mdPaginator.pageSize === undefined ? 25 : this.mdPaginator.pageSize;
        let query = <ElasticQuery>{
            from: pageSize * this.mdPaginator.pageIndex,
            size: pageSize,
            sort: sortObject
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
        return this.tvgMediaService.list(query);
    }

    disconnect() { }
}