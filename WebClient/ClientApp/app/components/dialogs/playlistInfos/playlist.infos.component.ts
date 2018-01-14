import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { PlaylistService } from '../../../services/playlists/playlist.service';
import { PlaylistModel, PlaylistPostModel } from '../../../types/playlist.type';
import { TvgMedia } from '../../../types/media.type';
import { CommonService } from '../../../services/common/common.service';
import { XtreamService } from '../../../services/xtream/xtream.service';

/**
*  url tests
 * http://www.m3uliste.pw/
 */
@Component({
    selector: 'playlist-infos-dialog',
    templateUrl: './playlist.infos.dialog.html'
})
export class PlaylistInfosDialog implements OnInit, OnDestroy {
    constructor(
        public dialogRef: MatDialogRef<PlaylistInfosDialog>, private playlistService: PlaylistService, private commonService: CommonService,
        @Inject(MAT_DIALOG_DATA) public playlist: PlaylistModel, private xtreamService: XtreamService, private snackBar: MatSnackBar) { }

    ngOnInit(): void {
        if (this.playlist.xtreamPlayerApi == null) {
            this.xtreamService.getUserAndServerInfo(this.playlist.publicId)
                .subscribe(xtreamPlayerApi => {
                    this.playlist.xtreamPlayerApi = xtreamPlayerApi;
                }, error => {
                    this.snackBar.open("This playlist does'nt xtream compatible");
                    this.dialogRef.close();
                });
        }
    }

    onNoClick(): void {
        this.dialogRef.close();
    }

    ngOnDestroy() {
    }
}
