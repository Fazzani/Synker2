﻿import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject, AfterViewInit } from '@angular/core';
import { DataSource } from '@angular/cdk/collections';
import { MatPaginator, PageEvent, MatSort, MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar, MatTableDataSource, MatSelect, MatAutocompleteTrigger, MatSelectionListChange } from '@angular/material';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subject } from 'rxjs/Subject';
import { Subscription } from 'rxjs/Subscription';
import { Observable } from "rxjs/Observable";
import 'rxjs/add/observable/fromEvent';
import { distinctUntilChanged, merge, debounceTime } from 'rxjs/operators';
import { CommonService, Constants } from '../../services/common/common.service';
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import { EventTargetLike } from "rxjs/observable/FromEventObservable";
import { PlaylistModel, PlaylistPostModel } from "../../types/playlist.type";
import { PlaylistService } from "../../services/playlists/playlist.service";
import { ActivatedRoute } from '@angular/router';
import { TvgMedia, Tvg, TvgSource, MediaType } from "../../types/media.type";
import { TvgMediaModifyDialog } from '../media/media.component';
import { MediaRefService } from '../../services/mediaref/mediaref.service';
import { FormControl } from '@angular/forms';
import { KEY_CODE, KEY, PageListState } from '../../types/common.type';
import { snakbar_duration } from '../../variables';
import { sitePackChannel } from '../../types/sitepackchannel.type';
import { PiconService } from '../../services/picons/picons.service';
import { SitePackService } from '../../services/sitepack/sitepack.service';
import { KeysPipe } from '../../pipes/enumKey.pipe';
import { EventEmitter } from 'events';
import { TvgMediaService } from '../../services/tvgmedia/tvgmedia.service';
import { PlaylistBulkUpdate } from '../dialogs/playlistBulkUpdate/playlistBulkUpdate';
import { PlaylistTvgSitesDialog } from '../dialogs/playlistTvgSites/PlaylistTvgSitesDialog';
import { PlaylistDiffDialog } from '../dialogs/playlistDiff/playlist.diff.component';
import { GroupsDialog } from '../dialogs/group/groups.component';
import { MatchTvgDialog } from '../dialogs/matchTvg/matchTvg.component';

@Component({
    selector: 'playlist',
    templateUrl: './playlist.component.html',
    providers: [PlaylistService],
    host: {
        '(window:keyup)': 'handleKeyboardEvent($event)'
    }
})
/** mediaref component*/
export class PlaylistComponent implements OnInit, OnDestroy, AfterViewInit {
    manualPage: number | null;
    key: number;
    subscriptionTableEvent: Subscription;
    columns = [
        { columnDef: 'position', header: 'Position', cell: (row: TvgMedia) => `${row.position}`, showed: true, actionColumn: false },
        { columnDef: 'tvg.logo', header: 'Logo', cell: (row: TvgMedia) => `${row.tvg.logo}`, showed: true, actionColumn: false, isImage: true },
        { columnDef: 'name', header: 'Name', cell: (row: TvgMedia) => `${row.name}`, showed: true, actionColumn: false },
        { columnDef: 'displayName', header: 'DisplayName', cell: (row: TvgMedia) => `${row.displayName}`, showed: true, actionColumn: false },
        { columnDef: 'lang', header: 'Lang', cell: (row: TvgMedia) => `${row.lang}`, showed: true, actionColumn: false },
        { columnDef: 'group', header: 'Group', cell: (row: TvgMedia) => `${row.group}`, showed: true, actionColumn: false },
        { columnDef: 'tvg.name', header: 'Tvg name', cell: (row: TvgMedia) => `${row.tvg.name}`, showed: true, actionColumn: false },
        { columnDef: 'tvg.tvgIdentify', header: 'Tvg id', cell: (row: TvgMedia) => `${row.tvg.tvgIdentify}`, showed: true, actionColumn: false },
        { columnDef: 'actions', header: 'Actions', cell: (row: TvgMedia) => ``, showed: true, actionColumn: true }
    ];

    columnDefs = this.columns.filter(x => !x.actionColumn && x.showed);
    displayedColumns = this.columns.map(x => x.columnDef);

    @ViewChild(MatPaginator) paginator: MatPaginator;
    @ViewChild(MatSort) sort: MatSort;
    @ViewChild('filter') filter: ElementRef;
    dataSource: MatTableDataSource<TvgMedia> | null;
    routeSub: any;
    playlistId: string;
    playlistBS: BehaviorSubject<PlaylistModel> | null;
    pagelistState: PageListState;
    mouseDown: boolean = false;
    select: boolean = false;
    atLeastOneSelected: BehaviorSubject<number> = new BehaviorSubject<number>(0);

    /** media ctor */
    constructor(private route: ActivatedRoute, private piconService: PiconService, private playlistService: PlaylistService,
        private sitePackService: SitePackService, private commonService: CommonService, private tvgMediaService: TvgMediaService,
        public dialog: MatDialog, public snackBar: MatSnackBar) {

    }

    /** Called by Angular after media component initialized */
    ngOnInit(): void {

        this.playlistBS = new BehaviorSubject<PlaylistModel>(null);
        this.dataSource = new MatTableDataSource<TvgMedia>([]);

        this.paginator.pageSizeOptions = [50, 100, 250, 1000];

        this.routeSub = this.route.params.subscribe(params => {
            this.playlistId = params['id']; // (+) converts string 'id' to a number
            console.log('Loading playlist ', this.playlistId);
            this.pagelistState = this.commonService.JsonToObject<PageListState>(localStorage.getItem(Constants.MediaPageListKey + this.playlistId))
            if (this.pagelistState == null)
                this.pagelistState = <PageListState>{ filter: "", pageIndex: 1, pageSize: this.paginator.pageSizeOptions[0] };

            console.log('pagelistState => ', this.pagelistState);

            this.initPaginator();

            // In a real app: dispatch action to load the details here.
            this.playlistService.get(this.playlistId, false).subscribe(x => {
                this.playlistBS.next(x);
            });
        });

        this.playlistBS.subscribe(x => {
            console.log('playlist updated');
            if (x != null && x.tvgMedias != null) {
                this.dataSource = new MatTableDataSource<TvgMedia>(x.tvgMedias);
                this.dataSource.filterPredicate =
                    (data: TvgMedia, filter: string) => {
                        filter = filter.toLowerCase();
                        let values = filter.split(":");
                        if (values.length > 1 && data[values[0].trim()]) {
                            return data[values[0].trim()].trim().toLowerCase().indexOf(values[1].toLowerCase().trim()) != -1;
                        } else {
                            return data.name.toLowerCase().indexOf(filter) != -1;
                        }
                    };
                this.initPaginator();
            }
        });

        this.subscriptionTableEvent = this.paginator.page.asObservable()
            .merge(Observable.fromEvent<EventTargetLike>(this.filter.nativeElement, 'keyup'))
            .debounceTime(1000)
            .distinctUntilChanged()
            .subscribe((x) => {

                if (!this.dataSource) { return; }
                console.log("subscriptionTableEvent => ", x);
                this.paginator.pageIndex = 0;
                if (x as PageEvent && (x as PageEvent).pageIndex !== undefined) {
                    this.paginator.pageIndex = (x as PageEvent).pageIndex;
                }

                this.dataSource.paginator = this.paginator;
                this.pagelistState.pageIndex = this.dataSource.paginator.pageIndex;
                this.pagelistState.pageSize = this.dataSource.paginator.pageSize;

                this.pagelistState.filter = this.filter.nativeElement.value;
                this.dataSource.filter = (this.pagelistState.filter as string).toLowerCase().trim();
                localStorage.setItem(Constants.MediaPageListKey + this.playlistId, JSON.stringify(this.pagelistState));
            });
    }

    private initPaginator() {
        this.paginator.pageIndex = this.pagelistState.pageIndex;
        this.paginator.pageSize = this.pagelistState.pageSize;
        this.dataSource!.paginator = this.paginator;
        this.dataSource!.sort = this.sort;
        this.dataSource!.filter = this.filter.nativeElement.value = this.pagelistState.filter;
    }

    reload(): void {
        this.ngOnInit();
    }

    //#region row selection
    handleKeyboardEvent(event: KeyboardEvent) {
        console.log(event);
        //Select ALL
        if (event.key == KEY.A && event.ctrlKey) {
            console.log('select All', this.dataSource.filteredData.length);
            this.dataSource
                ._pageData(this.dataSource.filteredData)
                .forEach(m => m.selected = true);
            this.atLeastOneSelected.next(this.dataSource.data.filter(f => f.selected).length);
        } else if (event.key == KEY.I && event.ctrlKey) {
            this.dataSource
                ._pageData(this.dataSource.filteredData)
                .forEach(m => m.selected = !m.selected);
            this.atLeastOneSelected.next(this.dataSource.data.filter(f => f.selected).length);
        }
    }

    onSelectionStart(position) {
        this.mouseDown = true;
        this.select = !this.dataSource.data.filter(x => x.position == position)[0].selected;
    }
    onSelection(position) {
        if (!this.mouseDown)
            return;

        console.log("Selected!", this.select, position);
        this.dataSource.data.filter(x => x.position == position)[0].selected = this.select;
    }
    onSelectionEnd() {
        this.mouseDown = false;
        this.atLeastOneSelected.next(this.dataSource.data.filter(f => f.selected).length);
    }

    /**
     * Select All media present in current playlist
     */
    selectAll() {
        this.dataSource.data.forEach(x => x.selected = true);
        this.atLeastOneSelected.next(this.dataSource.data.filter(f => f.selected).length);
    }

    /**
     * Toggle Selection on selected medias in current playlist
     * @param media
     * @param event
     */
    toggleSelected(media: TvgMedia, event: any): void {
        //console.log('toggleSelected ', media);
        //console.log(event);
        if (!event.ctrlKey) {
            this.dataSource.data.filter((v, i) => v.id != media.id).forEach((m, i) => {
                m.selected = false;
            });
        }
        media.selected = !media.selected;

        this.atLeastOneSelected.next(this.dataSource.data.filter(f => f.selected).length);
    }

    //#endregion

    ngAfterViewInit() {
        console.log('ngAfterViewInit _____________________________________________');
    }

    ngOnDestroy() {
        // this.subscriptionTableEvent.unsubscribe();
        this.routeSub.unsubscribe();
    }

    //#region media position AND Organization
    /**
     * Switch media position
     * @param {TvgMedia} x
     * @param {TvgMedia} y
     */
    switchPosition(x: number, y: number) {
        let p = this.dataSource.data[x];
        this.dataSource.data[x].position = this.dataSource.data[y].position;
        this.dataSource.data[y].position = p.position;
        this.dataSource.data[x] = this.dataSource.data[y];
        this.dataSource.data[y] = p;
    }

    moveUp = (index: number) => {
        let selectedItems = this.dataSource.data.filter(x => x.selected);
        selectedItems.forEach((x, i) => {
            let index = this.dataSource.data.indexOf(x);
            let index2 = index + selectedItems.length;
            this.switchPosition(index, index2);
        });
    };

    moveDown = (index: number) => {
        let selectedItems = this.dataSource.data.filter(x => x.selected);
        selectedItems.forEach((x, i) => {
            let index = this.dataSource.data.indexOf(x);
            let index2 = index + selectedItems.length;
            this.switchPosition(index, index2);
        });
    };

    /**
     * Reorgonize medias
     * @returns
     */
    organizeMedias(): void {
        this.commonService.displayLoader(true);

        let counter: number = 1;
        Observable
            .from(this.dataSource.data)
            .groupBy(x => x.group)
            .mergeMap(x => x
                .toArray()
                .map(m => m.sort((a, b) => a.displayName === b.displayName ? 0 : a.displayName > b.displayName ? 1 : -1)))
            .flatMap(x => x)
            .do(x => console.log(x.group))
            .map((t, i) => {
                t.position = counter++;
                return t;
            }).subscribe(x => this.commonService.displayLoader(false));
    }

    trackByPosition = (index, item) => +item.position;
    //#endregion


    // #region dialogs

    openBulkUpdateDialog(): void {
        let dialogRef = this.dialog.open(PlaylistBulkUpdate, {
            width: '550px',
            data: [
                this.dataSource.data.filter((v, i) => v.selected),
                Observable.from(this.dataSource.data.filter(f => f.group != null).map(x => x.group)).distinct().toArray()]
        });

        dialogRef.afterClosed().subscribe(result => {
            this.snackBar.open("List media modified", "", { duration: snakbar_duration });
        });
    }

    openDialog(media: TvgMedia): void {

        if (media.tvg == null) {
            media.tvg = <Tvg>{};
            media.tvg.tvgSource = <TvgSource>{};
        }
        let dialogRef = this.dialog.open(TvgMediaModifyDialog, {
            width: '550px',
            data: [media, this.playlistBS.getValue().tvgSites]
        });

        dialogRef.afterClosed().subscribe(result => {
            this.snackBar.open(media.displayName + " was modified", "", { duration: snakbar_duration });
        });
    }

    openGroupsDialog(): void {
        let dialogRef = this.dialog.open(GroupsDialog, {
            width: '550px',
            data: this.playlistBS.getValue()
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result != null) {
                this.filter.nativeElement.value = `group : ${result}`;
                this.paginator.page.emit(<PageEvent>{ pageIndex: 0 });;
            }
        });
    }

    openDiffPlaylist(): void {
        let dialogRef = this.dialog.open(PlaylistDiffDialog, {
            width: '550px',
            data: this.playlistBS.getValue()
        });

        dialogRef.afterClosed().subscribe(result => {
            this.snackBar.open("Tvg Sites was modified", "", { duration: snakbar_duration });
        });
    }

    openUpdateTvgSite(): void {
        let dialogRef = this.dialog.open(PlaylistTvgSitesDialog, {
            width: '550px',
            data: this.playlistBS.getValue()
        });

        dialogRef.afterClosed().subscribe(result => {
            this.snackBar.open("Tvg Sites was modified", "", { duration: snakbar_duration });
        });
    }

    openMatchTvgDialog(): void {
        let dialogRef = this.dialog.open(MatchTvgDialog, {
            width: '800px',
            data: [this.dataSource.data, this.playlistBS.getValue().tvgSites]
        });

        dialogRef.afterClosed().subscribe((result: Array<TvgMedia>) => {
            this.snackBar.open("Matching Tvg finished", "", { duration: snakbar_duration });
        });
    }

    // #endregion

    //#region CRUD

    save(): void {
        console.log("saving playlist..");
        this.playlistService.update(this.playlistBS.getValue()).subscribe(res => {
            this.snackBar.open("Playlist was saved successfully");
        });
    }

    deleteSelected(): void {
        const confirm = window.confirm('Do you really want to delete all selected medias?');
        Observable
            .of("")
            .filter(() => confirm)
            .switchMap(x => {
                this.dataSource.data = this.dataSource.data.filter(x => !x.selected);
                this.playlistBS.next(this.playlistBS.value);
                return x;
            })
            .subscribe(res => this.snackBar.open("Selected Media was deleted"));
    }

    delete(id: string): void {
        const confirm = window.confirm('Do you really want to delete this media?');
        let mediaIndex = this.dataSource.data.findIndex(x => x.id == id);
        console.log(`media ${id} with index ${mediaIndex} removed`);
        Observable
            .of(id)
            .filter(() => confirm)
            .switchMap(x => {
                this.dataSource.data.splice(mediaIndex, 1);
                this.playlistBS.next(this.playlistBS.value);
                return x;
            })
            .subscribe(res => this.snackBar.open("Media was deleted"));
    }

    //#endregion

    /**
    * Synchronize all picons from github
    */
    synkPiconsGlobal(): void {

        this.piconService.synk().subscribe(res => {
            this.commonService.info('Success', "Picons index was synchronized");
        });
    }

    /**
     * Force reset from source
     */
    reset(): void {
        this.playlistService.synk(new PlaylistPostModel()).subscribe(res => {
            this.playlistBS.next(res);
            this.commonService.info('Success', "Playlist was synchronized with source");
        });
    }

    /**
     * Execute handlers on selected medias
     */
    executeHandlers(): void {
        this.playlistService.executeHandlers(this.dataSource.data.filter((v, i) => v.selected)).subscribe(res => {
            res.forEach(x => {
                var index = this.playlistBS.value.tvgMedias.findIndex(f => f.id == x.id);

                if (index >= 0) {
                    this.playlistBS.value.tvgMedias[index] = x;
                    console.log('executeHandlers media : ', x);
                }
            });
            this.playlistBS.next(this.playlistBS.value);
            this.commonService.info('Success', "Executing handlers finished");
        });
    }

    //#region Matching

    /**
     * Mach picons
     * @param {number = 90} distance minimum should match
     * @param {boolean = true} shouldMatchChannelNumber should Match Channel Number
     */
    matchPicons(distance: number = 90, shouldMatchChannelNumber: boolean = true): void {
        this.piconService.match(this.dataSource.data.filter((v, i) => v.selected), distance, shouldMatchChannelNumber).subscribe(res => {
            res.forEach(x => {
                var index = this.playlistBS.value.tvgMedias.findIndex(f => f.id == x.id);

                if (index >= 0) {
                    this.playlistBS.value.tvgMedias[index] = x;
                    console.log('match picons media : ', x);
                }
            });
            this.playlistBS.next(this.playlistBS.value);
            this.commonService.info('Success', "Matching picons finished");
        });
    }

    //#region TMDB VOD
    /**
     * Try match selected videos  VOD informations
     */
    matchVideos(): void {
        this.playlistService.matchVideos(...this.dataSource.data.filter((v, i) => v.selected)).subscribe(res => {
            res.forEach(x => {
                var index = this.playlistBS.value.tvgMedias.findIndex(f => f.id == x.id);

                if (index >= 0) {
                    this.playlistBS.value.tvgMedias[index] = x;
                    console.log('match picons media : ', x);
                }
            });
            this.playlistBS.next(this.playlistBS.value);
            this.commonService.info('Success', "Playlist was matched with all VOD");
        });
    }

    /**
     * Try match media VOD information
     */
    matchVideo(media: TvgMedia): void {
        this.playlistService.matchVideo(media.displayName).subscribe(res => {
            this.commonService.info('Success', `Matching with VOD ${media.displayName} ${res}`);
            media.tvg.logo = res.posterPath;
        });
    }
    //#endregion

    //#endregion

    /**
     * Group medias
     */
    groupMedias(): void {
        Observable.from(this.dataSource.data)
            .groupBy(x => x.group)
            .mergeMap(group => group.toArray());
    }

    /**
     * GO TO PAGE
     * @param index
     */
    updateManualPage(index) {
        console.log(`go to page ${index}`);
        this.paginator.page.emit(<PageEvent>{ pageIndex: index });
    }
}

