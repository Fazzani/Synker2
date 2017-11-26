import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject } from '@angular/core';
import { PlaylistService } from '../../services/playlists/playlist.service';
import { CommonService } from '../../services/common/common.service';
import { MatDialog, MatSnackBar } from '@angular/material';
import { PlaylistModel } from '../../types/playlist.type';
import { QueryListBaseModel, PagedResult } from '../../types/common.type';

@Component({
    selector: 'home',
    templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit, OnDestroy {
    playlists: PagedResult<PlaylistModel>;
    query: QueryListBaseModel;
    constructor(private playlistService: PlaylistService, private commonService: CommonService, public dialog: MatDialog, public snackBar: MatSnackBar) { }

    ngOnInit(): void {
        this.query = new QueryListBaseModel();
        this.query.pageNumber = 0;
        this.query.pageSize = 20;
        this.playlistService.list(this.query).subscribe(x => {
            this.playlists = x;
        });
    }

    ngOnDestroy() {
    }
}
