import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { Observable } from 'rxjs/Observable';
import { GroupedObservable } from 'rxjs/operators/groupBy';
import { CommonService } from '../../../services/common/common.service';
import { PlaylistModel } from '../../../types/playlist.type';
import { PlaylistService } from '../../../services/playlists/playlist.service';

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

    filterByGroup = (group) => this.dialogRef.close(group);

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
