import { Resolve } from '@angular/router';
import { ActivatedRouteSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { PlaylistService } from '../../services/playlists/playlist.service';
import { of } from 'rxjs';

@Injectable()
export class PlaylistDetailResolver implements Resolve<any> {
  constructor(private playlistService: PlaylistService) {}

  resolve(route: ActivatedRouteSnapshot) {
    return this.playlistService.get(route.paramMap.get('id'), false);
  }
}
