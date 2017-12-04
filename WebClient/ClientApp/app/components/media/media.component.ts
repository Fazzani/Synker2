import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject } from '@angular/core';
import { DataSource } from '@angular/cdk/collections';
import { MatPaginator, PageEvent, MatSort, MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subject } from 'rxjs/Subject';
import { Subscription } from 'rxjs/Subscription';

import { TvgMediaService } from '../../services/tvgmedia/tvgmedia.service';
import { CommonService, Constants } from '../../services/common/common.service';
import { Observable } from "rxjs/Observable";
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import 'rxjs/add/observable/fromEvent';
import { map, catchError, merge, debounceTime, distinctUntilChanged, switchMap, startWith } from 'rxjs/operators';

import { EventTargetLike } from "rxjs/observable/FromEventObservable";
import { TvgMedia } from '../../types/media.type';
import { MediaRefService } from '../../services/mediaref/mediaref.service';
import { mediaRef } from '../../types/mediaref.type';

@Component({
    selector: 'app-media',
    templateUrl: './media.component.html',
    providers: [TvgMediaService, CommonService]
})
export class MediaComponent implements OnInit, OnDestroy {
    subscriptionTableEvent: Subscription;

    displayedColumns = ['position', 'name', 'group', 'lang', 'actions'];
    @ViewChild(MatPaginator) paginator: MatPaginator;
    @ViewChild(MatSort) sort: MatSort;
    @ViewChild('filter') filter: ElementRef;
    dataSource: MediaDataSource | null;
    currentItem: TvgMedia | null;

    /** media ctor */
    constructor(private tvgMediaService: TvgMediaService, private commonService: CommonService, public dialog: MatDialog, public snackBar: MatSnackBar) { }

    /** Called by Angular after media component initialized */
    ngOnInit(): void {
        let storedQuery = this.commonService.JsonToObject<ElasticQuery>(localStorage.getItem(Constants.LS_MediaQueryKey));
        console.log("storedQuery ", storedQuery);
        this.paginator.pageIndex = storedQuery != null ? Math.floor(storedQuery.from / storedQuery.size) : 0;
        this.paginator.pageSizeOptions = [50, 100, 250, 1000];
        this.paginator.pageSize = storedQuery != null ? storedQuery.size : this.paginator.pageSizeOptions[0];
        this.filter.nativeElement.value = storedQuery != null && storedQuery.query != null && storedQuery.query != {} ? JSON.stringify(storedQuery.query) : "";

        if (storedQuery != null && storedQuery.sort !== undefined) {
            this.sort.active = Object.keys(storedQuery.sort)[0];
            this.sort.direction = storedQuery.sort[this.sort.active].order;
        }
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
export class TvgMediaModifyDialog implements OnInit, OnDestroy {
    tvgMedias: Observable<mediaRef[]>;
    private searchTerms = new Subject<string>();  
    private filter: string;

    constructor(
        public dialogRef: MatDialogRef<TvgMediaModifyDialog>,
        private mediarefService: MediaRefService,
        @Inject(MAT_DIALOG_DATA) public media: TvgMedia) { }

    ngOnInit(): void {

        this.tvgMedias = this.searchTerms
            .debounceTime(1000)         
            .distinctUntilChanged()   
            .switchMap(term => term  
                ? this.mediarefService.simpleSearch<mediaRef>(term, "mediaref").map(x => x.result)
                : Observable.of<mediaRef[]>([]))
            .catch(error => {
                console.log(error);
                return Observable.of<mediaRef[]>([]);
            });  
    }

    tvgSelectionChange(event: any, mediaref: mediaRef): void {
        this.media.tvg = mediaref.tvg;
    }

    search(term: string): void {
        this.searchTerms.next(term);
    }  

    save(): void {
        this.dialogRef.close();
    }
    onNoClick(): void {
        this.dialogRef.close();
    }

    ngOnDestroy(): void {
    }
}

/**
 * Media datasource for mat-table component
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

    constructor(private tvgMediaService: TvgMediaService, private MatPaginator: MatPaginator, private _sort: MatSort) {
        super();

        // this._filterChange
        //     .merge(this.MatPaginator)
        //     .debounceTime(300)
        //     //.distinctUntilChanged()
        //     .subscribe((x) => this.getData());
    }

    connect(): Observable<TvgMedia[]> {
        const displayDataChanges = [
            this._sort.sortChange,
            this.MatPaginator.page,
            this.MatPaginator.pageSize
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

        let pageSize = this.MatPaginator.pageSize === undefined ? 25 : this.MatPaginator.pageSize;
        let query = <ElasticQuery>{
            from: pageSize * this.MatPaginator.pageIndex,
            size: pageSize,
            sort: sortObject
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
        return this.tvgMediaService.list(query);
    }

    disconnect() { }
}