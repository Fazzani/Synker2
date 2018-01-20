import { Component, OnInit, OnDestroy, Inject, EventEmitter } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar, MatSelectionListChange } from '@angular/material';
import { Observable } from 'rxjs/Observable';
import { GroupedObservable } from 'rxjs/operators/groupBy';
import { CommonService } from '../../../services/common/common.service';
import { PlaylistModel } from '../../../types/playlist.type';
import { PlaylistService } from '../../../services/playlists/playlist.service';
import { TvgMedia } from '../../../types/media.type';

@Component({
    selector: 'groups-dialog',
    templateUrl: './groups.dialog.html'
})
export class GroupsDialog implements OnInit, OnDestroy {
    groupMedias$: Observable<{ title: string; medias: TvgMedia[]; count: number; selected: boolean; }[]>;

    constructor(
        public dialogRef: MatDialogRef<GroupsDialog>, @Inject(MAT_DIALOG_DATA) public playlist: PlaylistModel, private playlistService: PlaylistService,
        private commonService: CommonService) { }

    ngOnInit(): void {
        this.groupMedias$ = Observable.from(this.playlist.tvgMedias)
            .groupBy(x => x.mediaGroup.name)
            .mergeMap(group =>
                group.reduce((acc, tvgmedia) => { acc.push(tvgmedia); return acc; }, [])
                    .map(tvgmedias => ({ title: group.key || "Untitled", medias: tvgmedias, count: tvgmedias.length, selected: tvgmedias ? !tvgmedias[0].mediaGroup.disabled : true })))
            .toArray();
    }

    onSelectedOptionsChange(selectedOptionChanged: MatSelectionListChange): void {
        let value = <{ title: string; medias: TvgMedia[]; count: number; selected: boolean; }>selectedOptionChanged.option.value;
        value.selected = !value.selected;
        value.medias.forEach(x => x.mediaGroup.disabled = !selectedOptionChanged.option.selected);
    }

    toggleSelectionAll(selected: boolean): void {
        //selected ? groups.selectAll() : groups.deselectAll();
        this.groupMedias$.forEach(x => x.forEach(m => {
            m.selected = selected;
            m.medias.forEach(media => media.mediaGroup.disabled = !selected)
        }));
    }

    filterByGroup = (group) => this.dialogRef.close(group);

    onNoClick(): void {
        this.dialogRef.close();
    }

    ngOnDestroy() {
    }
}
