import { from, Observable } from 'rxjs';
import { mergeMap, reduce, map, groupBy, toArray } from 'rxjs/operators';
import { Component, OnInit, OnDestroy, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA, MatSelectionListChange } from "@angular/material";
import { PlaylistModel } from "../../../types/playlist.type";
import { TvgMedia } from "../../../types/media.type";

@Component({
  selector: "groups-dialog",
  templateUrl: "./groups.dialog.html"
})
export class GroupsDialog implements OnInit, OnDestroy {
  groupMedias$: Observable<{ title: string; medias: TvgMedia[]; count: number; selected: boolean }[]>;

  constructor(
    public dialogRef: MatDialogRef<GroupsDialog>,
    @Inject(MAT_DIALOG_DATA) public playlist: PlaylistModel
  ) {}

  ngOnInit(): void {
    this.groupMedias$ = from(this.playlist.tvgMedias).pipe(
      groupBy(x => x.mediaGroup.name),
      mergeMap(group =>
        group.pipe(
          reduce((acc:any, tvgmedia) => {
            acc.push(tvgmedia);
            return acc;
          }),
          map(tvgmedias => ({
            title: group.key || "Untitled",
            medias: tvgmedias,
            count: tvgmedias.length,
            selected: tvgmedias ? !tvgmedias[0].mediaGroup.disabled : true
          })),)
      ),
      toArray(),);
  }

  onSelectedOptionsChange(selectedOptionChanged: MatSelectionListChange): void {
    let value = <
      { title: string; medias: TvgMedia[]; count: number; selected: boolean }
    >selectedOptionChanged.option.value;
    value.selected = !value.selected;
    value.medias.forEach(
      x => (x.mediaGroup.disabled = !selectedOptionChanged.option.selected)
    );
  }

  toggleSelectionAll(selected: boolean): void {
    //selected ? groups.selectAll() : groups.deselectAll();
    this.groupMedias$.forEach(x =>
      x.forEach(m => {
        m.selected = selected;
        m.medias.forEach(media => (media.mediaGroup.disabled = !selected));
      })
    );
  }

  filterByGroup = group => this.dialogRef.close(group);

  onNoClick(): void {
    this.dialogRef.close();
  }

  ngOnDestroy() {}
}
