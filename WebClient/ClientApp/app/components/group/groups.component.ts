import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { CommonService, Constants } from '../../services/common/common.service';
import { PlaylistModel, PlaylistStatus, PlaylistPostModel, Providers } from "../../types/playlist.type";
import { PlaylistService } from "../../services/playlists/playlist.service";
import { Observable } from 'rxjs/Observable';
import { GroupedObservable } from 'rxjs/operators/groupBy';
import { TvgMedia } from '../../types/media.type';

@Component({
    selector: 'groups-dialog',
    templateUrl: './groups.dialog.html'
})
export class GroupsDialog implements OnInit, OnDestroy {
    groupMedias: Observable<{ group: string; count: number; }[]>;

    constructor(
        public dialogRef: MatDialogRef<GroupsDialog>, @Inject(MAT_DIALOG_DATA) public playlist: PlaylistModel, private playlistService: PlaylistService,
        private commonService: CommonService) { }

    ngOnInit(): void {
        this.groupMedias = this.getGroupMedias();
    }

    /**
     * Group medias
     */
    getGroupMedias = () =>
        Observable.from(this.playlist.tvgMedias)
            .groupBy(x => x.group).mergeMap(group => group
                .count()
                .map(total => ({ group: group.key, count: total }))
            )
            .toArray()

    onNoClick(): void {
        this.dialogRef.close();
    }

    ngOnDestroy() {
    }
}
