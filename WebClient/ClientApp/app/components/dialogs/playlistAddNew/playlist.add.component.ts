﻿import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { PlaylistService } from '../../../services/playlists/playlist.service';
import { CommonService } from '../../../services/common/common.service';
import { PlaylistPostModel, PlaylistStatus, Providers } from '../../../types/playlist.type';

/**
*  url tests
 * http://www.m3uliste.pw/
 */
@Component({
    selector: 'playlist-add-dialog',
    templateUrl: './playlist.add.dialog.html'
})
export class PlaylistAddDialog implements OnInit, OnDestroy {
    providers: string[];

    playlist: PlaylistPostModel;
    
    constructor(
        public dialogRef: MatDialogRef<PlaylistAddDialog>, private playlistService: PlaylistService, private commonService: CommonService) { }

    ngOnInit(): void {
        this.playlist = new PlaylistPostModel();
        this.playlist.status = PlaylistStatus.enabled;
        this.playlist.provider = "m3u";
        this.providers = Object.keys(Providers);
    }

    onNoClick(): void {
        this.dialogRef.close();
    }

    create(): void {
        this.playlistService.addByUrl(this.playlist).subscribe(ok => {
            this.dialogRef.close();
        });
    }

    ngOnDestroy() {
    }
}