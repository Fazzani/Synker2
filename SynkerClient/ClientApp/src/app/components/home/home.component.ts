import { of } from "rxjs";
import { switchMap, filter } from "rxjs/operators";
import { Component, OnInit, OnDestroy } from "@angular/core";
import { PlaylistService } from "../../services/playlists/playlist.service";
import { MatDialog, MatSnackBar } from "@angular/material";
import { PlaylistModel } from "../../types/playlist.type";
import { QueryListBaseModel, PagedResult } from "../../types/common.type";
import { ClipboardService } from "ngx-clipboard";
import { PlaylistAddDialog } from "../dialogs/playlistAddNew/playlist.add.component";
import { PlaylistInfosDialog } from "../dialogs/playlistInfos/playlist.infos.component";
import { ActivatedRoute } from "@angular/router";

@Component({
  selector: "home",
  templateUrl: "./home.component.html"
})
export class HomeComponent implements OnInit, OnDestroy {
  playlists: PagedResult<PlaylistModel> = <PagedResult<PlaylistModel>>{
    results: new Array<PlaylistModel>()
  };
  query: QueryListBaseModel;

  constructor(
    private route: ActivatedRoute,
    private playlistService: PlaylistService,
    public dialog: MatDialog,
    public snackBar: MatSnackBar,
    private clipboardService: ClipboardService
  ) {}

  ngOnInit(): void {
    this.query = <QueryListBaseModel>{ pageNumber: 0, pageSize: 20 };
    this.playlists = <PagedResult<PlaylistModel>>this.route.snapshot.data.data;
  }

  openPlaylistInfosDialog(playlist: PlaylistModel): void {
    let dialogRef = this.dialog.open(PlaylistInfosDialog, {
      width: "700px",
      data: playlist
    });
  }

  share(playlist: PlaylistModel): void {}

  copyPublicLink(link: string): void {
    if (this.clipboardService.isSupported) this.clipboardService.copyFromContent(link);
  }

  delete(playlist: PlaylistModel): void {
    const confirm = window.confirm(`Do you really want to delete this playlist ${playlist.freindlyname}?`);

    of(playlist.publicId)
      .pipe(
        filter(() => confirm),
        switchMap(x => this.playlistService.delete(x))
      )
      .subscribe(res => {
        this.snackBar.open("Playlist deleted successfully");
        this.ngOnInit();
      });
  }

  /**
   * open Create new playlist dialog
   */
  openDialogAddNewPlaylist(): void {
    let dialogRef = this.dialog.open(PlaylistAddDialog, {
      width: "700px"
    });

    dialogRef.afterClosed().subscribe(result => {
      this.ngOnInit();
    });
  }

  ngOnDestroy() {
    this.clipboardService.destroy();
  }
}
