import { Component, OnInit, OnDestroy, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { PlaylistService } from "../../../services/playlists/playlist.service";
import { PlaylistModel } from "../../../types/playlist.type";
import { CommonService } from "../../../services/common/common.service";
import { XtreamService } from "../../../services/xtream/xtream.service";

/**
 *  url tests
 * http://www.m3uliste.pw/
 */
@Component({
  selector: "playlist-infos-dialog",
  templateUrl: "./playlist.infos.dialog.html"
})
export class PlaylistInfosDialog implements OnInit, OnDestroy {
  providersEnum: string[] = PlaylistModel.PROVIDERS;
  PlaylistStatusVaLues: string[] = PlaylistModel.STATUS;
  SynkGroupEnumValues: string[] = PlaylistModel.SYNKGROUP;

  constructor(
    public dialogRef: MatDialogRef<PlaylistInfosDialog>,
    private playlistService: PlaylistService,
    private commonService: CommonService,
    @Inject(MAT_DIALOG_DATA) public playlist: PlaylistModel,
    private xtreamService: XtreamService
  ) {
    this.playlist = this.playlist || <PlaylistModel>{};
  }

  ngOnInit(): void {
    console.log(this.playlist);
    if (this.playlist.xtreamPlayerApi == null) {
      this.xtreamService.getUserAndServerInfo(this.playlist.publicId).subscribe(
        xtreamPlayerApi => {
          this.playlist.xtreamPlayerApi = xtreamPlayerApi;
        },
        error => {
          //this.snackBar.open("This playlist does'nt xtream compatible");
          //this.dialogRef.close();
        }
      );
    }
  }

  save(): void {
    this.playlistService
      .updateLight(this.playlist)
      .subscribe(x =>
        this.commonService.success(`Updating playlist ${this.playlist.freindlyname}`, `the playlist ${this.playlist.freindlyname} saved succesfully`)
      );
  }

  onNoClick(): void {
    this.dialogRef.close();
  }

  ngOnDestroy() { }
}
