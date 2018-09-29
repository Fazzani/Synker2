import { debug } from 'util';
import { tap, mergeMap } from "rxjs/operators";
import { OnInit, OnDestroy, Inject, Component } from "@angular/core";
import { sitePackChannel } from "../../../types/sitepackchannel.type";
import { PlaylistService } from "../../../services/playlists/playlist.service";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { SitePackService } from "../../../services/sitepack/sitepack.service";
import { PlaylistModel } from "../../../types/playlist.type";

@Component({
  selector: "tvgsites-list-modify-dialog",
  templateUrl: "./tvgsites.list.dialog.html"
})
export class PlaylistTvgSitesDialog implements OnInit, OnDestroy {
  public tvgSites: sitePackChannel[] = [];
  public query: string;

  constructor(
    private playlistService: PlaylistService,
    private sitePackService: SitePackService,
    public dialogRef: MatDialogRef<PlaylistTvgSitesDialog>,
    @Inject(MAT_DIALOG_DATA) public data: PlaylistModel
  ) {}

  ngOnInit(): void {
    this.sitePackService
      .tvgSites()
      .pipe(
        mergeMap(m => m),
        tap(x => {
          x.selected = this.data.tvgSites.findIndex(f => f == x.site) >= 0;
        })
      )
      .subscribe(m => {
        this.tvgSites.push(m);
        this.tvgSites.sort(this.compareFn);
      });
  }

  save(): void {
    this.data.tvgSites = this.tvgSites.filter(x => x.selected).map(x => x.site);
    this.playlistService.updateLight(this.data).subscribe(ok => this.dialogRef.close());
  }

  compareFn = (a: sitePackChannel, b: sitePackChannel) => (a.selected === b.selected ? 0 : a.selected ? -1 : 1);

  onNoClick(): void {
    this.dialogRef.close();
  }

  ngOnDestroy(): void {}
}
