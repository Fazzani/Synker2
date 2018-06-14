import { Resolve } from "@angular/router";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Injectable } from "@angular/core";
import { PlaylistService } from "../../services/playlists/playlist.service";
import { QueryListBaseModel, PagedResult } from "../../types/common.type";
import { Observable } from "rxjs";
import { PlaylistModel } from "../../types/playlist.type";

@Injectable()
export class HomeResolver implements Resolve<Observable<PagedResult<PlaylistModel>>> {
  constructor(private playlistService: PlaylistService) {}

  resolve(route: ActivatedRouteSnapshot) {
    return this.playlistService.list(<QueryListBaseModel>{ pageNumber: 0, pageSize: 20 });
  }
}
