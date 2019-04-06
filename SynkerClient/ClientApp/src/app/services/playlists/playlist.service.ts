import { Observable, merge } from "rxjs";
import { catchError, map, flatMap, filter } from "rxjs/operators";
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { AngularFireDatabase } from "@angular/fire/database";
import { BaseService } from "../base/base.service";
import { HttpHeaders, HttpParams } from "@angular/common/http";
import { PlaylistModel, PlaylistPostModel, PlaylistModelLive } from "../../types/playlist.type";
import { QueryListBaseModel, PagedResult } from "../../types/common.type";
import { TvgMedia, MediaGroup } from "../../types/media.type";
import { environment } from "../../../environments/environment";
import { FirebasePlaylistHealthState } from "../../types/firebase.type";

@Injectable()
export class PlaylistService extends BaseService {
  constructor(protected http: HttpClient, private db: AngularFireDatabase) {
    super(http);
    this._baseUrl = "playlists";
  }

  get(id: string, light: boolean): Observable<PlaylistModel> {
    let params = new HttpParams();
    params = params.set("light", light ? "true" : "false");

    return this.http
      .get(`${environment.base_api_url}${this._baseUrl}/${id}`, {
        params: params
      })
      .pipe(
        map(this.handleSuccess)
      );
  }

  list(query: QueryListBaseModel): Observable<PagedResult<PlaylistModel>> {
    return this.http.post(`${environment.base_api_url}${this._baseUrl}/search`, query).pipe(
      map(this.handleSuccess)
    );
  }

  /**
   * playlist list with live status from firebase
   */
  listWithHealthStatus(playlistsToWatch: PlaylistModelLive[]): Observable<PlaylistModelLive> {
    const promises = playlistsToWatch.map(pl => {
      //console.warn(`playlist to watch from firebase: ${JSON.stringify(pl)}`);
      return this.db.object<FirebasePlaylistHealthState>(`/playlisthealthstate/${pl.id}`).valueChanges();
    });
    return merge(promises).pipe(
      flatMap(x => x),
      filter(x => x != null),
      map(x => <PlaylistModelLive>{ id: x.Id, isOnline: x.IsOnline, mediaCount: x.MediaCount, freindlyname: x.Name })
    );
  }

  synk(model: PlaylistPostModel): Observable<PlaylistModel> {
    return this.http.post(`${environment.base_api_url}${this._baseUrl}/synk`, model).pipe(
      map(this.handleSuccess)
    );
  }

  executeHandlers(model: TvgMedia[]): Observable<TvgMedia[]> {
    return this.http.post(`${environment.base_api_url}${this._baseUrl}/handlers`, model).pipe(
      map(this.handleSuccess)
    );
  }

  match(id: string): Observable<PlaylistModel> {
    return this.http.post(`${environment.base_api_url}${this._baseUrl}/match/$${id}`, null).pipe(
      map(this.handleSuccess)
    );
  }

  matchtvg(id: string, onlyNotMatched: boolean = true): Observable<PlaylistModel> {
    return this.http.post(`${environment.base_api_url}${this._baseUrl}/matchtvg/${id}?onlyNotMatched=${onlyNotMatched}`, null).pipe(
      map(this.handleSuccess)
    );
  }

  matchFiltredTvgSites(id: string, onlyNotMatched: boolean = true): Observable<PlaylistModel> {
    return this.http.post(`${environment.base_api_url}${this._baseUrl}/matchfiltred/${id}?onlyNotMatched=${onlyNotMatched}`, null).pipe(
      map(this.handleSuccess)
    );
  }

  /**
   * Match VOD Playlist
   * @param {string} id
   * @returns
   */
  matchVideosByPlaylist(id: string): Observable<PlaylistModel> {
    return this.http.post(`${environment.base_api_url}${this._baseUrl}/matchvideos/${id}`, null).pipe(
      map(this.handleSuccess)
    );
  }

  /**
   * Match videos VOD
   * @param {string} id
   * @returns
   */
  matchVideos(...medias: TvgMedia[]): Observable<TvgMedia[]> {
    return this.http.post(`${environment.base_api_url}${this._baseUrl}/matchvideos`, medias).pipe(
      map(this.handleSuccess)
    );
  }

  /**
   * Match VOD by name
   * @param {string} mediaName
   * @returns Poster Paht
   */
  matchVideo(mediaName: string): Observable<any> {
    return this.http.post(`${environment.base_api_url}${this._baseUrl}/matchvideo/${mediaName}`, null).pipe(
      map(this.handleSuccess)
    );
  }

  //export(fromType: string, toType: string, ): Observable<any> {
  //    return this.http.post(environment.base_api_url + 'playlists/export/' + fromType + '/' + toType, null).map(res => {
  //        return res;
  //    }).catch(this.handleError);
  //}

  diff(model: PlaylistPostModel): Observable<PlaylistModel> {
    return this.http.post(`${environment.base_api_url}${this._baseUrl}/diff`, model).pipe(
      map(this.handleSuccess)
    );
  }

  addByUrl(p: PlaylistPostModel): Observable<PlaylistModel> {
    return this.http
      .post(`${environment.base_api_url}${this._baseUrl}/create`, p, {
        headers: new HttpHeaders().set("Content-Type", "application/json"),
        responseType: "text"
      })
      .pipe(
        map(this.handleSuccess)
      );
  }

  addByStream(p: PlaylistPostModel): Observable<PlaylistModel> {
    return this.http
      .post(`${environment.base_api_url}${this._baseUrl}/create/m3u`, p, {
        headers: new HttpHeaders().set("Content-Type", "application/json"),
        responseType: "text"
      })
      .pipe(
        map(this.handleSuccess)
      );
  }

  update(p: PlaylistModel): Observable<PlaylistModel> {
    return this.http
      .put(`${environment.base_api_url}${this._baseUrl}/${p.publicId}`, p, {
        headers: new HttpHeaders().set("Content-Type", "application/json"),
        responseType: "text"
      })
      .pipe(
        map(this.handleSuccess)
      );
  }

  updateLight(p: PlaylistModel): Observable<any> {
    let playlist: PlaylistModel = Object.assign({}, p);
    playlist.tvgMedias = null;
    return this.http
      .put(`${environment.base_api_url}${this._baseUrl}/light/${p.publicId}`, playlist, {
        headers: new HttpHeaders().set("Content-Type", "application/json"),
        responseType: "text"
      })
      .pipe();
  }

  /**
   * Get playlist groups by id
   * @param id playlist id
   */
  groups(id: string): Observable<MediaGroup[]> {
    return this.http.get(`${environment.base_api_url}${this._baseUrl}/${id}/groups`).pipe(
      map(this.handleSuccess)
    );
  }

  /**
   * Get group children tvgmedia
   * @param id playlist id
   * @param group group name
   */
  childrenGroups(id: string, group: string): Observable<TvgMedia[]> {
    return this.http.get(`${environment.base_api_url}${this._baseUrl}/${id}/groups/${group}`).pipe(
      map(this.handleSuccess)
    );
  }
}
