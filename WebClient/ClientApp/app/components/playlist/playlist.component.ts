import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject, AfterViewInit } from '@angular/core';
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
import { TvgMedia } from "../../types/media.type";
import { TvgMediaModifyDialog } from '../media/media.component';
import { MediaRefService } from '../../services/mediaref/mediaref.service';
import { FormControl } from '@angular/forms';
import { KEY_CODE, KEY } from '../../types/common.type';
import { mediaRef } from "../../types/mediaref.type";
import { PlaylistDiffDialog } from './playlist.diff.component';
import { snakbar_duration } from '../../variables';

@Component({
    selector: 'playlist',
    templateUrl: './playlist.component.html',
    providers: [PlaylistService, CommonService],
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
    /** media ctor */
    constructor(private route: ActivatedRoute, private playlistService: PlaylistService, private mediaRefService: MediaRefService, private commonService: CommonService,
        public dialog: MatDialog, public snackBar: MatSnackBar) { }

    /** Called by Angular after media component initialized */
    ngOnInit(): void {
        this.playlistBS = new BehaviorSubject<PlaylistModel>(null);
        this.dataSource = new MatTableDataSource<TvgMedia>([]);

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
            }
        });

        this.paginator.pageIndex = 0;
        this.paginator.pageSizeOptions = [50, 100, 250, 1000];
        this.paginator.pageSize = this.paginator.pageSizeOptions[0];
        this.dataSource.paginator = this.paginator;
        //this.filter.nativeElement.value = storedQuery != null && storedQuery.query != null && storedQuery.query != {} ? JSON.stringify(storedQuery.query) : "";
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

    reload(): void {
        this.ngOnInit();
    }

    handleKeyboardEvent(event: KeyboardEvent) {
        console.log(event);
        //Select ALL
        if (event.key == KEY.A && event.ctrlKey) {
            this.dataSource.
                _pageData(this.dataSource.data)
                .forEach(m => m.selected = true);
        } else if (event.key == KEY.I && event.ctrlKey) {
            this.dataSource.
                _pageData(this.dataSource.data)
                .forEach(m => m.selected = !m.selected);
        }
    }

    ngAfterViewInit() {
        console.log('ngAfterViewInit _____________________________________________');
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
    }

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

    update(playlist: PlaylistPostModel): void {

    }

    delete(id: string): void {
        // this.dataSource.delete(id);
        /**
         * .subscribe(res => {
                this.snackBar.open("media ref removed");
            });
         */
    }

    synk(): void {
        this.playlistService.synk(new PlaylistPostModel()).subscribe(res => {
            this.playlistBS.next(res);
            this.snackBar.open("Playlist was synchronized with source");
        });
    }

    executeHandlers(): void {
        this.playlistService.executeHandlers(this.dataSource.data.filter((v, i) => v.selected)).subscribe(res => {
            res.forEach(x => {
                var index = this.playlistBS.value.tvgMedias.findIndex(f => f.id == x.id);
                if (index > 0) {
                    this.playlistBS.value.tvgMedias[index] = x;
                }
            });
            this.playlistBS.next(this.playlistBS.value);
            this.snackBar.open("Executing handlers finished");
        });
    }

    match(): void {
        this.playlistService.match(this.playlistBS.getValue().publicId).subscribe(res => {
            this.playlistBS.next(res);
            this.snackBar.open("Playlist was matched with all MediaRef");
        });
    }

    ///**
    // * add media playlist to mediaRef
    // * @param media
    // */
    //addToMediaRef(media: TvgMedia): void {
    //    this.mediaRefService.save(new mediaRef(media.displayName, media.lang)).subscribe(res => {
    //        this.snackBar.open("media was added to mediaRef successfully");
    //    });
    //}

    matchFiltredTvgSites(onlyNotMatched: boolean = false): void {
        this.playlistService.matchFiltredTvgSites(this.playlistBS.getValue().publicId, onlyNotMatched).subscribe(res => {
            this.playlistBS.next(res);
            this.snackBar.open("Playlist was matched with filtred TvgSites");
        });
    }

    save(): void {
        console.log("saving playlist..");
        this.playlistService.update(this.playlistBS.getValue()).subscribe(res => {
            this.snackBar.open("Playlist was saved successfully");
        });
    }

    selectAll() {
        this.dataSource.data.forEach(x => x.selected = true);
    }

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

    ngOnDestroy() {
        // this.subscriptionTableEvent.unsubscribe();
        this.routeSub.unsubscribe();
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
    cultures: string[];
    selected: string;
    group: string;
    filterChannelName: string;

    constructor(private mediaRefService: MediaRefService,
        public dialogRef: MatDialogRef<TvgMediaListModifyDialog>,
        @Inject(MAT_DIALOG_DATA) public data: TvgMedia[]) { }

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

    save(): void {
        this.dialogRef.close();
    }
    onNoClick(): void {
        this.dialogRef.close();
    }

    
    applyFixChannelName(replace: string): void {
        this.data
            .filter(x => new RegExp(this.filterChannelName).test(x.displayName))
            .forEach(x => x.displayName = x.displayName.replace(new RegExp(this.filterChannelName), replace));
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
            .flatMap(m => m.map(c => <any>({ name: c, selected: false })))
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
