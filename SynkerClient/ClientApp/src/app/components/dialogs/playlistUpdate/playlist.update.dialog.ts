import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material";
import { Inject, Component } from "@angular/core";
import { PlaylistModel } from "../../../types/playlist.type";

@Component({
  selector: "playlist-update-dialog",
  templateUrl: "./playlist.update.dialog.html"
})
export class PlaylistUpdateDialog {
  constructor(public dialogRef: MatDialogRef<PlaylistUpdateDialog>, @Inject(MAT_DIALOG_DATA) public data: PlaylistModel) {}

  onNoClick(): void {
    this.dialogRef.close();
  }

  save(): void {
    console.log("BulkUpdate saving");
    // this.playlistService.updateLight(this.data).subscribe(ok => this.dialogRef.close());
  }
}
