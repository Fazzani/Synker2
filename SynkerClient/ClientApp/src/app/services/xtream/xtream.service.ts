import { Observable } from "rxjs";
import { catchError, map } from 'rxjs/operators';
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import { Epg_Listings, Channels, Live, XtreamPanel, PlayerApi } from "../../types/xtream.type";

@Injectable({
  providedIn: 'root',
})
export class XtreamService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http);
    this._baseUrl = "xtream";
  }

  public getAllEpg(playlistId: string): Observable<Epg_Listings[]> {
    return this.http
      .get(`${this.FullBaseUrl}/allepg/playlist/${playlistId}`).pipe(
      map(this.handleSuccess));
  }
  public getLiveCategories(playlistId: string): Observable<Live[]> {
    return this.http
      .get(`${this.FullBaseUrl}/livecats/playlist/${playlistId}`).pipe(
      map(this.handleSuccess));
  }
  public getLiveStreams(playlistId: string): Observable<Channels[]> {
    return this.http
      .get(`${this.FullBaseUrl}/livestreams/playlist/${playlistId}`).pipe(
      map(this.handleSuccess));
  }
  public getLiveStreamsByCategories(playlistId: string, catId: number): Observable<Channels[]> {
    return this.http
      .get(`${this.FullBaseUrl}/livestreams/playlist/${playlistId}}/${catId}`).pipe(
      map(this.handleSuccess));
  }
  public getPanel(playlistId: string): Observable<XtreamPanel> {
    return this.http
      .get(`${this.FullBaseUrl}/allepg/panel/${playlistId}`).pipe(
      map(this.handleSuccess));
  }
  public getShortEpgForStream(playlistId: string, streamId: number): Observable<Epg_Listings[]> {
    return this.http
      .get(`${this.FullBaseUrl}/panel/playlist/${playlistId}/${streamId}`).pipe(
      map(this.handleSuccess));
  }
  public getUserAndServerInfo(playlistId: string): Observable<PlayerApi> {
    return this.http
      .get(`${this.FullBaseUrl}/infos/playlist/${playlistId}`).pipe(
      map(this.handleSuccess));
  }
  public getVodStreams(playlistId: string): Observable<Channels[]> {
    return this.http
      .get(`${this.FullBaseUrl}/vods/playlist/${playlistId}`).pipe(
      map(this.handleSuccess));
  }
  public getXmltv(playlistId: string): Observable<Epg_Listings[]> {
    return this.http
      .get(`${this.FullBaseUrl}/xmltv/playlist/${playlistId}`).pipe(
      map(this.handleSuccess));
  }
}
