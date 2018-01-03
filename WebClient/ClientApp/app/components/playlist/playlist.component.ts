import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject, AfterViewInit } from '@angular/core';
import { DataSource } from '@angular/cdk/collections';
import { MatPaginator, PageEvent, MatSort, MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar, MatTableDataSource, MatSelect, MatAutocompleteTrigger } from '@angular/material';
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
import { mediaRef } from "../../types/mediaref.type";
import { PlaylistDiffDialog } from './playlist.diff.component';
import { snakbar_duration } from '../../variables';
import { sitePackChannel } from '../../types/sitepackchannel.type';
import { PiconService } from '../../services/picons/picons.service';
import { SitePackService } from '../../services/sitepack/sitepack.service';
import { KeysPipe } from '../../pipes/enumKey.pipe';
import { EventEmitter } from 'events';

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

    displayedColumns = ['position', 'tvg.logo', 'name', 'displayName', 'lang', 'group', 'tvg.name', 'tvg.tvgIdentify', 'actions'];
    @ViewChild(MatPaginator) paginator: MatPaginator;
    @ViewChild(MatSort) sort: MatSort;
    @ViewChild('filter') filter: ElementRef;
    dataSource: MatTableDataSource<TvgMedia> | null;
    routeSub: any;
    playlistId: string;
    playlistBS: BehaviorSubject<PlaylistModel> | null;
    pagelistState: PageListState;
    /** media ctor */
    constructor(private route: ActivatedRoute, private piconService: PiconService, private playlistService: PlaylistService,
        private sitePackService: SitePackService, private commonService: CommonService,
        public dialog: MatDialog, public snackBar: MatSnackBar) { }

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
                if (x as PageEvent) {
                    if ((x as PageEvent).pageIndex !== undefined)
                        this.paginator.pageIndex = (x as PageEvent).pageIndex;
                    else
                        if ((x as PageEvent).length === undefined)
                            this.paginator.pageIndex = 1;
                }

                this.dataSource.paginator = this.paginator;
                this.pagelistState.pageIndex = this.dataSource.paginator.pageIndex;
                this.pagelistState.pageSize = this.dataSource.paginator.pageSize;
                this.dataSource.filter = this.pagelistState.filter = this.filter.nativeElement.value;
                localStorage.setItem(Constants.MediaPageListKey + this.playlistId, JSON.stringify(this.pagelistState));
            });
    }

    private initPaginator() {
        this.paginator.pageIndex = this.pagelistState.pageIndex;
        this.paginator.pageSize = this.pagelistState.pageSize;
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.dataSource.filter = this.filter.nativeElement.value = this.pagelistState.filter;
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
    }

    ngOnDestroy() {
        // this.subscriptionTableEvent.unsubscribe();
        this.routeSub.unsubscribe();
    }

    // #region dialogs

    openUpdateListDialog(): void {
        let dialogRef = this.dialog.open(TvgMediaListModifyDialog, {
            width: '550px',
            data: [
                this.dataSource.data.filter((v, i) => v.selected),
                Observable.from(this.dataSource.data.map(x => x.group.toLowerCase()).filter(x => x != null)).distinct().toArray()]
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
            this.snackBar.open("Picons index was synchronized");
        });
    }

    synk(): void {
        this.playlistService.synk(new PlaylistPostModel()).subscribe(res => {
            this.playlistBS.next(res);
            this.snackBar.open("Playlist was synchronized with source");
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
            this.snackBar.open("Executing handlers finished");
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
            this.snackBar.open("Matching picons finished");
        });
    }

    //#region TMDB VOD
    /**
     * Try match playlist VOD information
     */
    matchVideos(): void {
        this.playlistService.matchVideos(this.playlistBS.getValue().publicId).subscribe(res => {
            this.playlistBS.next(res);
            this.snackBar.open("Playlist was matched with all VOD");
        });
    }

    /**
     * Try match media VOD information
     */
    matchVideo(media: TvgMedia): void {
        this.playlistService.matchVideo(media.displayName).subscribe(res => {
            this.snackBar.open(`Matching with VOD ${media.displayName} ${res}`);
            media.tvg.logo = res.posterPath;
        });
    }
    //#endregion

    /**
     * Try match tvg with Sites defined in media
     */
    matchTvg(): void {
        this.playlistService.matchtvg(this.playlistBS.getValue().publicId, false).subscribe(res => {
            this.playlistBS.next(res);
            this.snackBar.open("Playlist was matched with all sitepacks");
        });
    }

    /**
     * Match with TvgSites defined on the playlist
     * @param onlyNotMatched
     */
    matchFiltredTvgSites(onlyNotMatched: boolean = false): void {
        this.playlistService.matchFiltredTvgSites(this.playlistBS.getValue().publicId, onlyNotMatched).subscribe(res => {
            this.playlistBS.next(res);
            this.snackBar.open("Playlist was matched with filtred TvgSites");
        });
    }

    /**
     * Find tvg for median name  and country
     * @param {TvgMedia} media
     */
    matchTvgByMediaAndCountry(media: TvgMedia): void {

        this.sitePackService.matchTvgByMedia(media.displayName, media.lang).subscribe(sitepack => {
            if (sitepack != null) {
                media.tvg.name = sitepack.channel_name;
                media.tvg.id = sitepack.xmltv_id;
            }
        });
    }
    //#endregion

    /**
     * Group channels
     */
    groupMedias(): void {
        const source = Observable.from(this.dataSource.data);

        source
            .groupBy(x => x.group)
            .mergeMap(group => group.toArray());

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

    /**
     * GO TO PAGE
     * @param index
     */
    updateManualPage(index) {
        console.log(`go to page ${index}`);
        this.paginator.page.emit(<PageEvent>{ pageIndex: index });
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
    mediaTypes: typeof MediaType;
    sitePacks: sitePackChannel[];
    cultures: string[];
    selectedLang: string;
    keyUpSitePack = new Subject<any>();
    filterChannelName: string;
    selectedMediaType: MediaType;
    enabled: boolean = true;
    data: TvgMedia[];

    groupsfiltred: string[];
    groups: string[];
    group: string;
    searchGroups$ = new Subject<KeyboardEvent>();
    @ViewChild(MatAutocompleteTrigger) autoTrigger: MatAutocompleteTrigger;

    constructor(private sitePackService: SitePackService, private commonService: CommonService,
        public dialogRef: MatDialogRef<TvgMediaListModifyDialog>,
        @Inject(MAT_DIALOG_DATA) public tup: [TvgMedia[], Observable<string[]>]) {

        this.data = tup[0];
        tup[1].subscribe(x => this.groups = x);

        this.mediaTypes = MediaType;

        // AutoComplete sitepacks
        const subscription = this.keyUpSitePack
            .map(event => '*' + event.target.value + '*')
            .debounceTime(500)
            .distinctUntilChanged()
            .subscribe(search => sitePackService.sitePacks(search).subscribe(res => this.sitePacks = res));

        //AutoComplete groups
        this.searchGroups$
            .filter(x => x.keyCode != 13)
            .map(m => m.key.toLowerCase())
            .distinctUntilChanged()
            .do(() => { this.groupsfiltred = [] })
            .map(m => this.groups.filter(f => f.toLowerCase().indexOf(this.group) >= 0))
            .distinct()
            .subscribe(x => {
                console.log(x);
                this.groupsfiltred = x;
            });

        this.searchGroups$
            .filter(x => x.keyCode == 13)
            .subscribe(x => {
                console.log(this.group);
                this.autoTrigger.closePanel();
                this.onChangeGroup(this.group);
            });
    }

    ngOnInit(): void {

        if (this.data != undefined && this.data.length > 0) {
            this.selectedLang = this.data[0].lang;
            this.selectedMediaType = this.data[0].mediaType;
        }

        this.sitePackService.countries().subscribe(c => {
            this.cultures = c;
        });
    }

    onChangeLang(event): void {
        console.log("culture was changed : ", this.selectedLang);
        this.data.forEach(m => m.lang = this.selectedLang);
    }

    onChangeGroup(g): void {
        console.log("Group was changed : ", g);
        this.data.forEach(m => m.group = g);
    }

    onChangeEnabled(enabled: boolean): void {
        console.log("disabled was changed : ", enabled);
        this.data.forEach(m => m.enabled = enabled);
    }

    onChangeMediaType(mediaType: number): void {
        console.log("MediaType was changed : ", mediaType);
        this.data.forEach(m => m.mediaType = mediaType);
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
//---------------------------------------------------------------------------------   TvgSites ListModifyDialog
@Component({
    selector: 'tvgsites-list-modify-dialog',
    templateUrl: './tvgsites.list.dialog.html'
})
export class TvgSitesListModifyDialog implements OnInit, OnDestroy {
    tvgSites: any[] = [];

    constructor(private playlistService: PlaylistService, private sitePackService: SitePackService,
        public dialogRef: MatDialogRef<TvgSitesListModifyDialog>,
        @Inject(MAT_DIALOG_DATA) public data: PlaylistModel) {
    }

    ngOnInit(): void {
        this.sitePackService.tvgSites()
            .flatMap(m => m.map(c => <any>({ id: c.id, site: c.site, country: c.country, selected: false })))
            .do(x => x.selected = this.data.tvgSites.findIndex(f => f == x.site) >= 0)
            .subscribe(m => this.tvgSites.push(m));
    }

    onChange(event): void {
    }

    save(): void {
        console.log('Saving TvgSites');
        this.data.tvgSites = this.tvgSites.filter(x => x.selected).map(x => x.site);
        this.playlistService.updateLight(this.data).subscribe(ok => this.dialogRef.close());
    }
    onNoClick(): void {
        this.dialogRef.close();
    }

    ngOnDestroy(): void {
    }
}
