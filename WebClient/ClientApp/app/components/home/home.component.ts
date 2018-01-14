import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject, Renderer } from '@angular/core';
import { PlaylistService } from '../../services/playlists/playlist.service';
import { CommonService } from '../../services/common/common.service';
import { MatDialog, MatSnackBar } from '@angular/material';
import { PlaylistModel } from '../../types/playlist.type';
import { QueryListBaseModel, PagedResult } from '../../types/common.type';
import { ClipboardService } from 'ngx-clipboard';
import { Observable } from 'rxjs/Observable';
import { PlaylistAddDialog } from '../dialogs/playlistAddNew/playlist.add.component';
import { XtreamService } from '../../services/xtream/xtream.service';
import { PlaylistInfosDialog } from '../dialogs/playlistInfos/playlist.infos.component';

@Component({
    selector: 'home',
    templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit, OnDestroy {
    playlists: PagedResult<PlaylistModel> = <PagedResult<PlaylistModel>>{ results: new Array<PlaylistModel>() };
    query: QueryListBaseModel;

    constructor(private renderer: Renderer, private playlistService: PlaylistService, private commonService: CommonService, public dialog: MatDialog,
        public snackBar: MatSnackBar, private clipboardService: ClipboardService) { }

    ngOnInit(): void {

        this.query = new QueryListBaseModel();
        this.query.pageNumber = 0;
        this.query.pageSize = 20;
        this.playlistService.list(this.query).subscribe(x => {
            this.playlists = x;
        });
    }

    openPlaylistInfosDialog(playlist: PlaylistModel): void {
        let dialogRef = this.dialog.open(PlaylistInfosDialog, {
            width: '400px',
            data: playlist
        });
    }

    share(playlist: PlaylistModel): void {
           
    }

    copyPublicLink(link: string): void {
        if (this.clipboardService.isSupported)
            this.clipboardService.copyFromContent(link, this.renderer);
    }

    delete(playlist: PlaylistModel): void {
        const confirm = window.confirm(`Do you really want to delete this playlist ${playlist.freindlyname}?`);

        Observable
            .of(playlist.publicId)
            .filter(() => confirm)
            .switchMap(x => this.playlistService.delete(x))
            .subscribe(res => {
                this.snackBar.open("Playlist was deleted");
                this.ngOnInit();
            });
    }

    openDialogAddNewPlaylist(): void {
        let dialogRef = this.dialog.open(PlaylistAddDialog, {
            width: '550px'
        });

        dialogRef.afterClosed().subscribe(result => {
            this.ngOnInit();
        });
    }
    ngOnDestroy() {
        this.clipboardService.destroy();
    }
}
