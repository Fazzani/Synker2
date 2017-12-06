import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject, Renderer } from '@angular/core';
import { PlaylistService } from '../../services/playlists/playlist.service';
import { CommonService } from '../../services/common/common.service';
import { MatDialog, MatSnackBar } from '@angular/material';
import { PlaylistModel } from '../../types/playlist.type';
import { QueryListBaseModel, PagedResult } from '../../types/common.type';
import { ClipboardService } from 'ngx-clipboard';
import { PlaylistAddDialog } from '../playlist/playlist.add.component';

@Component({
    selector: 'home',
    templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit, OnDestroy {
    playlists: PagedResult<PlaylistModel>;
    query: QueryListBaseModel;
    constructor(private renderer: Renderer, private playlistService: PlaylistService, private commonService: CommonService, public dialog: MatDialog, public snackBar: MatSnackBar, private clipboardService: ClipboardService) { }

    ngOnInit(): void {

        this.query = new QueryListBaseModel();
        this.query.pageNumber = 0;
        this.query.pageSize = 20;
        this.playlistService.list(this.query).subscribe(x => {
            this.playlists = x;
        });
    }

    copyPublicLink(link: string): void {
        if (this.clipboardService.isSupported)
            this.clipboardService.copyFromContent(link, this.renderer);
    }
    ngOnDestroy() {
        this.clipboardService.destroy();
    }

    openDialogAddNewPlaylist(): void {
        let dialogRef = this.dialog.open(PlaylistAddDialog, {
            width: '550px'
        });

        dialogRef.afterClosed().subscribe(result => {
            this.ngOnInit();
        });
    }
}
