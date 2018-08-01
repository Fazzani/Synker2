import { Component, OnDestroy, OnInit } from "@angular/core";
import { MediaServerService } from "../../../services/admin/mediaserver.service";
import { MatSnackBar } from "@angular/material";
import { MediaServerStats } from "../../../types/mediaserver.stats.type";
import { Observable } from "rxjs";
import { MediaServerStreamsStats } from "../../../types/mediaserver.streams.stats.type";

@Component({
  selector: "admin-dashboard",
  templateUrl: "./admin.dashboard.component.html"
})
export class AdminDashboardComponent implements OnInit, OnDestroy {

  mediaServerStats$: Observable<MediaServerStats>;
  mediaServerStreamsStats$: Observable<MediaServerStreamsStats>;

  constructor(private mediaServerService: MediaServerService, public snackBar: MatSnackBar) {

  }

  ngOnInit(): void {
    this.mediaServerStats$ = this.mediaServerService.server();
    this.mediaServerStreamsStats$ = this.mediaServerService.streams();
  }
  ngOnDestroy(): void {
  }
}
