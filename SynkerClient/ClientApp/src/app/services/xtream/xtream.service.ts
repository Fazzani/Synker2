import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";

// All the RxJS stuff we need
import { Observable } from "rxjs/Rx";
import { map, catchError } from "rxjs/operators";
import { Epg_Listings, Channels, Live, XtreamPanel, PlayerApi } from "../../types/xtream.type";

@Injectable()
export class XtreamService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http, "xtream");
  }

  public getAllEpg(playlistId: string): Observable<Epg_Listings[]> {
    return this.http
      .get(`${this.FullBaseUrl}/allepg/playlist/${playlistId}`)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }
  public getLiveCategories(playlistId: string): Observable<Live[]> {
    return this.http
      .get(`${this.FullBaseUrl}/livecats/playlist/${playlistId}`)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }
  public getLiveStreams(playlistId: string): Observable<Channels[]> {
    return this.http
      .get(`${this.FullBaseUrl}/livestreams/playlist/${playlistId}`)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }
  public getLiveStreamsByCategories(playlistId: string, catId: number): Observable<Channels[]> {
    return this.http
      .get(`${this.FullBaseUrl}/livestreams/playlist/${playlistId}}/${catId}`)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }
  public getPanel(playlistId: string): Observable<XtreamPanel> {
    return this.http
      .get(`${this.FullBaseUrl}/allepg/panel/${playlistId}`)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }
  public getShortEpgForStream(playlistId: string, streamId: number): Observable<Epg_Listings[]> {
    return this.http
      .get(`${this.FullBaseUrl}/panel/playlist/${playlistId}/${streamId}`)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }
  public getUserAndServerInfo(playlistId: string): Observable<PlayerApi> {
    return this.http
      .get(`${this.FullBaseUrl}/infos/playlist/${playlistId}`)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }
  public getVodStreams(playlistId: string): Observable<Channels[]> {
    return this.http
      .get(`${this.FullBaseUrl}/vods/playlist/${playlistId}`)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }
  public getXmltv(playlistId: string): Observable<Epg_Listings[]> {
    return this.http
      .get(`${this.FullBaseUrl}/xmltv/playlist/${playlistId}`)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }
}
