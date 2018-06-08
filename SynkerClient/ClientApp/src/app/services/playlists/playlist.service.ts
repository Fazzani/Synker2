
import {catchError, map } from 'rxjs/operators';
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";

// All the RxJS stuff we need
import { Observable } from "rxjs";
import { HttpHeaders, HttpParams } from "@angular/common/http";
import { PlaylistModel, PlaylistPostModel } from "../../types/playlist.type";
import { QueryListBaseModel, PagedResult } from "../../types/common.type";
import { TvgMedia } from "../../types/media.type";
import { environment } from "../../../environments/environment";

@Injectable()
export class PlaylistService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http, "playlists");
  }

  get(id: string, light: boolean): Observable<PlaylistModel> {
    let params = new HttpParams();
    params = params.set("light", light ? "true" : "false");

    return this.http
      .get(`${environment.base_api_url}${this.BaseUrl}/${id}`, {
        params: params
      }).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  list(query: QueryListBaseModel): Observable<PagedResult<PlaylistModel>> {
    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/search`, query).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  synk(model: PlaylistPostModel): Observable<PlaylistModel> {
    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/synk`, model).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  executeHandlers(model: TvgMedia[]): Observable<TvgMedia[]> {
    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/handlers`, model).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  match(id: string): Observable<PlaylistModel> {
    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/match/$${id}`, null).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  matchtvg(id: string, onlyNotMatched: boolean = true): Observable<PlaylistModel> {
    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/matchtvg/${id}?onlyNotMatched=${onlyNotMatched}`, null).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  matchFiltredTvgSites(id: string, onlyNotMatched: boolean = true): Observable<PlaylistModel> {
    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/matchfiltred/${id}?onlyNotMatched=${onlyNotMatched}`, null).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  /**
   * Match VOD Playlist
   * @param {string} id
   * @returns
   */
  matchVideosByPlaylist(id: string): Observable<PlaylistModel> {
    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/matchvideos/${id}`, null).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  /**
   * Match videos VOD
   * @param {string} id
   * @returns
   */
  matchVideos(...medias: TvgMedia[]): Observable<TvgMedia[]> {
    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/matchvideos`, medias).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  /**
   * Match VOD by name
   * @param {string} mediaName
   * @returns Poster Paht
   */
  matchVideo(mediaName: string): Observable<any> {
    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/matchvideo/${mediaName}`, null).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  //export(fromType: string, toType: string, ): Observable<any> {
  //    return this.http.post(environment.base_api_url + 'playlists/export/' + fromType + '/' + toType, null).map(res => {
  //        return res;
  //    }).catch(this.handleError);
  //}

  diff(model: PlaylistPostModel): Observable<PlaylistModel> {
    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/diff`, model).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  addByUrl(p: PlaylistPostModel): Observable<PlaylistModel> {
    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/create`, p, {
        headers: new HttpHeaders().set("Content-Type", "application/json"),
        responseType: "text"
      }).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  addByStream(p: PlaylistPostModel): Observable<PlaylistModel> {
    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/create/m3u`, p, {
        headers: new HttpHeaders().set("Content-Type", "application/json"),
        responseType: "text"
      }).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  update(p: PlaylistModel): Observable<PlaylistModel> {
    return this.http
      .put(`${environment.base_api_url}${this.BaseUrl}/${p.publicId}`, p, {
        headers: new HttpHeaders().set("Content-Type", "application/json"),
        responseType: "text"
      }).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  updateLight(p: PlaylistModel): Observable<any> {
    return this.http
      .put(`${environment.base_api_url}${this.BaseUrl}/light/${p.publicId}`, p, {
        headers: new HttpHeaders().set("Content-Type", "application/json"),
        responseType: "text"
      }).pipe(
      catchError(this.handleError));
  }
}
