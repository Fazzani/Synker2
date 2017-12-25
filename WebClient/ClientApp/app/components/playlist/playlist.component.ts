﻿import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject, AfterViewInit } from '@angular/core';
import { DataSource } from '@angular/cdk/collections';
import { MatPaginator, PageEvent, MatSort, MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar, MatTableDataSource, MatSelect } from '@angular/material';
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
import { TvgMedia, Tvg, TvgSource } from "../../types/media.type";
import { TvgMediaModifyDialog } from '../media/media.component';
import { MediaRefService } from '../../services/mediaref/mediaref.service';
import { FormControl } from '@angular/forms';
import { KEY_CODE, KEY, PageListState } from '../../types/common.type';
import { mediaRef } from "../../types/mediaref.type";
import { PlaylistDiffDialog } from './playlist.diff.component';
import { snakbar_duration } from '../../variables';
import { sitePackChannel } from '../../types/sitepackchannel.type';

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
    key: number;
    subscriptionTableEvent: Subscription;

    displayedColumns = ['tvg.logo', 'name', 'displayName', 'lang', 'group', 'tvg.name', 'tvg.tvgIdentify', 'actions'];
    @ViewChild(MatPaginator) paginator: MatPaginator;
    @ViewChild(MatSort) sort: MatSort;
    @ViewChild('filter') filter: ElementRef;
    dataSource: MatTableDataSource<TvgMedia> | null;
    routeSub: any;
    playlistId: string;
    playlistBS: BehaviorSubject<PlaylistModel> | null;
    pagelistState: PageListState;
    /** media ctor */
    constructor(private route: ActivatedRoute, private playlistService: PlaylistService, private mediaRefService: MediaRefService, private commonService: CommonService,
        public dialog: MatDialog, public snackBar: MatSnackBar) { }

    /** Called by Angular after media component initialized */
    ngOnInit(): void {

        this.commonService.displayLoader(true);

        this.playlistBS = new BehaviorSubject<PlaylistModel>(null);
        this.dataSource = new MatTableDataSource<TvgMedia>([]);

        this.paginator.pageSizeOptions = [50, 100, 250, 1000];
        this.pagelistState = this.commonService.JsonToObject<PageListState>(localStorage.getItem(Constants.MediaPageListKey))
        if (this.pagelistState == null)
            this.pagelistState = <PageListState>{ filter: "", pageIndex: 0, pageSize: this.paginator.pageSizeOptions[0] };

        console.log('pagelistState => ', this.pagelistState);

        this.paginator.pageIndex = this.pagelistState.pageIndex;
        this.paginator.pageSize = this.pagelistState.pageSize;
        this.dataSource.paginator = this.paginator;

        this.routeSub = this.route.params.subscribe(params => {
            this.playlistId = params['id']; // (+) converts string 'id' to a number
            console.log('Loading playlist ', this.playlistId);
            // In a real app: dispatch action to load the details here.
            this.playlistService.get(this.playlistId, false).subscribe(x => {
                this.playlistBS.next(x);
            });
        });

        this.playlistBS.subscribe(x => {
            console.log('playlist updated');
            if (x != null && x.tvgMedias != null) {
                this.dataSource = new MatTableDataSource<TvgMedia>(x.tvgMedias);
                this.dataSource.paginator = this.paginator;
                this.dataSource.sort = this.sort;
                this.dataSource.filter = this.filter.nativeElement.value = this.pagelistState.filter;
                this.commonService.displayLoader(false);
            }
        });

        this.subscriptionTableEvent = this.paginator.page.asObservable()
            .merge(Observable.fromEvent<EventTargetLike>(this.filter.nativeElement, 'keyup'))
            .debounceTime(1000)
            .distinctUntilChanged()
            .subscribe((x) => {

                if (!this.dataSource) { return; }
                console.log("subscriptionTableEvent => ", x);
                if (x as PageEvent) {
                    if ((x as PageEvent).length === undefined)
                        this.paginator.pageIndex = 0;

                }
                this.pagelistState.pageIndex = this.dataSource.paginator.pageIndex;
                this.pagelistState.pageSize = this.dataSource.paginator.pageSize;
                this.dataSource.filter = this.pagelistState.filter = this.filter.nativeElement.value;
                localStorage.setItem(Constants.MediaPageListKey, JSON.stringify(this.pagelistState));
                this.dataSource.paginator = this.paginator;
            });
    }

    reload(): void {
        this.ngOnInit();
    }

    handleKeyboardEvent(event: KeyboardEvent) {
        console.log(event);
        //Select ALL
        if (event.key == KEY.A && event.ctrlKey) {
            console.log('select All', this.dataSource.filteredData.length);
            this.dataSource
                ._pageData(this.dataSource.filteredData)
                .forEach(m => m.selected = true);
        } else if (event.key == KEY.I && event.ctrlKey) {
            this.dataSource
                ._pageData(this.dataSource.filteredData)
                .forEach(m => m.selected = !m.selected);
        }
    }

    ngAfterViewInit() {
        console.log('ngAfterViewInit _____________________________________________');
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
    }

    ngOnDestroy() {
        // this.subscriptionTableEvent.unsubscribe();
        this.routeSub.unsubscribe();
    }

    // #region dialogs

    openUpdateListDialog(): void {
        let dialogRef = this.dialog.open(TvgMediaListModifyDialog, {
            width: '550px',
            data: this.dataSource.data.filter((v, i) => v.selected)
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
        let dialogRef = this.dialog.open(TvgSitesListModifyDialog, {
            width: '550px',
            data: this.playlistBS.getValue()
        });

        dialogRef.afterClosed().subscribe(result => {
            this.snackBar.open("Tvg Sites was modified", "", { duration: snakbar_duration });
        });
    }

    // #endregion

    //#region CRUD
    save(): void {
        this.commonService.displayLoader(true);
        console.log("saving playlist..");
        this.playlistService.update(this.playlistBS.getValue()).subscribe(res => {
            this.snackBar.open("Playlist was saved successfully");
            this.commonService.displayLoader(false);
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

    synk(): void {
        this.commonService.displayLoader(true);
        this.playlistService.synk(new PlaylistPostModel()).subscribe(res => {
            this.playlistBS.next(res);
            this.snackBar.open("Playlist was synchronized with source");
            this.commonService.displayLoader(false);
        });
    }

    /**
     * Execute handlers on selected medias
     */
    executeHandlers(): void {
        this.commonService.displayLoader(true);
        this.playlistService.executeHandlers(this.dataSource.data.filter((v, i) => v.selected)).subscribe(res => {
            res.forEach(x => {
                var index = this.playlistBS.value.tvgMedias.findIndex(f => f.id == x.id);

                if (index > 0) {
                    this.playlistBS.value.tvgMedias[index] = x;
                    console.log('executeHandlers media : ', x);
                }
            });
            this.playlistBS.next(this.playlistBS.value);
            this.snackBar.open("Executing handlers finished");
            this.commonService.displayLoader(false);
        });
    }

    /**
     * Mach picons
     */
    matchPicons(): void {
        this.commonService.displayLoader(true);
        this.playlistService.matchPicons(this.dataSource.data.filter((v, i) => v.selected)).subscribe(res => {

            res.forEach(x => {
                var index = this.playlistBS.value.tvgMedias.findIndex(f => f.id == x.id);

                if (index > 0) {
                    this.playlistBS.value.tvgMedias[index] = x;
                    console.log('match picons media : ', x);
                }
            });
            this.playlistBS.next(this.playlistBS.value);
            this.snackBar.open("Matching picons finished");
            this.commonService.displayLoader(false);
        });
    }

    /**
     * Match with all defined tvg in sitepack
     */
    matchTvg(): void {
        this.commonService.displayLoader(true);
        this.playlistService.matchtvg(this.playlistBS.getValue().publicId).subscribe(res => {
            this.playlistBS.next(res);
            this.snackBar.open("Playlist was matched with all MediaRef");
            this.commonService.displayLoader(false);
        });
    }

    /**
     * Match with TvgSites defined on the playlist
     * @param onlyNotMatched
     */
    matchFiltredTvgSites(onlyNotMatched: boolean = false): void {
        this.commonService.displayLoader(true);
        this.playlistService.matchFiltredTvgSites(this.playlistBS.getValue().publicId, onlyNotMatched).subscribe(res => {
            this.playlistBS.next(res);
            this.snackBar.open("Playlist was matched with filtred TvgSites");
            this.commonService.displayLoader(false);
        });
    }

    /**
     * Select All media present in current playlist
     */
    selectAll() {
        this.dataSource.data.forEach(x => x.selected = true);
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
    }

}

//---------------------------------------------------------------------------------    Playlist ModifyDialog
@Component({
    selector: 'playlist-modify-dialog',
    templateUrl: './playlist.dialog.html'
})
export class PlaylistModifyDialog {

    constructor(
        public dialogRef: MatDialogRef<PlaylistModifyDialog>,
        @Inject(MAT_DIALOG_DATA) public data: PlaylistModel[]) { }

    onNoClick(): void {
        this.dialogRef.close();
    }

    save(): void {
        console.log('BulkUpdate saving');
        // this.playlistService.updateLight(this.data).subscribe(ok => this.dialogRef.close());
    }
}
//---------------------------------------------------------------------------------    TvgMedia bulk ListModifyDialog
@Component({
    selector: 'tvgmedia-list-modify-dialog',
    templateUrl: './tvgmedia.list.dialog.html'
})
export class TvgMediaListModifyDialog implements OnInit, OnDestroy {
    sitePacks: sitePackChannel[];
    cultures: string[];
    selected: string;
    group: string;
    keyUpSitePack = new Subject<any>();
    filterChannelName: string;

    constructor(private mediaRefService: MediaRefService,
        public dialogRef: MatDialogRef<TvgMediaListModifyDialog>,
        @Inject(MAT_DIALOG_DATA) public data: TvgMedia[]) {

        const subscription = this.keyUpSitePack
            .map(event => '*' + event.target.value + '*')
            .debounceTime(1000)
            .distinctUntilChanged()
            .subscribe(search => mediaRefService.sitePacks(search).subscribe(res => this.sitePacks = res));
    }

    ngOnInit(): void {

        if (this.data != undefined && this.data.length > 0)
            this.selected = this.data[0].lang;

        this.mediaRefService.cultures().subscribe(c => {
            this.cultures = c;
        });
    }

    onChangeLang(event): void {
        console.log("culture was changed : ", this.selected);
        this.data.forEach(m => m.lang = this.selected);
    }

    onChangeGroup(event): void {
        console.log("Group was changed : ", this.group);
        this.data.forEach(m => m.group = this.group);
    }

    onChangeTvgSourceSite(sitePack: sitePackChannel): void {
        console.log("tvgSourceSite was changed : ", sitePack);
        this.data.forEach(m => {

            if (m.tvg == null)
                m.tvg = <Tvg>{};
            if (m.tvg.tvgSource == null)
                m.tvg.tvgSource = <TvgSource>{};

            m.tvg.tvgSource.site = sitePack.site;
            m.tvg.tvgSource.country = sitePack.country;
        });
    }

    save(): void {
        this.dialogRef.close();
    }
    onNoClick(): void {
        this.dialogRef.close();
    }

    applyFixChannelName(replace: string): void {
        this.data
            .filter(x => new RegExp(this.filterChannelName).test(x.displayName))
            .forEach(x => {
                x.displayName = x.displayName.replace(new RegExp(this.filterChannelName), replace);
            });
    }

    ngOnDestroy(): void {
    }
}
//---------------------------------------------------------------------------------    TvgSites ListModifyDialog
@Component({
    selector: 'tvgsites-list-modify-dialog',
    templateUrl: './tvgsites.list.dialog.html'
})
export class TvgSitesListModifyDialog implements OnInit, OnDestroy {
    tvgSites: any[] = [];

    constructor(private playlistService: PlaylistService, private mediaRefService: MediaRefService,
        public dialogRef: MatDialogRef<TvgSitesListModifyDialog>,
        @Inject(MAT_DIALOG_DATA) public data: PlaylistModel) {
    }

    ngOnInit(): void {
        this.mediaRefService.tvgSites()
            .flatMap(m => m.map(c => <any>({ id: c.id, site: c.site, country: c.country, selected: false })))
            .do(x => x.selected = this.data.tvgSites.findIndex(f => f == x.name) >= 0)
            .subscribe(m => this.tvgSites.push(m));
    }

    onChange(event): void {
    }

    save(): void {
        console.log('Saving TvgSites');
        this.data.tvgSites = this.tvgSites.filter(x => x.selected).map(x => x.name);
        this.playlistService.updateLight(this.data).subscribe(ok => this.dialogRef.close());
    }
    onNoClick(): void {
        this.dialogRef.close();
    }

    ngOnDestroy(): void {
    }
}
