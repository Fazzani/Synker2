import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { CommonService, Constants } from '../../services/common/common.service';
import { PlaylistModel, PlaylistStatus, PlaylistPostModel, Providers } from "../../types/playlist.type";
import { PlaylistService } from "../../services/playlists/playlist.service";
import { TvgMedia } from '../../types/media.type';

/**
*  url tests
 * http://www.m3uliste.pw/
 */
@Component({
    selector: 'playlist-diff-dialog',
    templateUrl: './playlist.diff.dialog.html'
})
export class PlaylistDiffDialog implements OnInit, OnDestroy {
    newMedias: TvgMedia[];
    removedMedias: TvgMedia[];
    constructor(
        public dialogRef: MatDialogRef<PlaylistDiffDialog>, private playlistService: PlaylistService, private commonService: CommonService,
        @Inject(MAT_DIALOG_DATA) public playlist: PlaylistPostModel) { }

    ngOnInit(): void {
        this.playlist.provider = "m3u";
        this.playlistService.diff(this.playlist).subscribe((res: any) => {
            this.newMedias = res.item1;
            this.removedMedias = res.item2;
        });
    }

    acceptMerge(): void {

    }

    onNoClick(): void {
        this.dialogRef.close();
    }

    ngOnDestroy() {
    }
}
