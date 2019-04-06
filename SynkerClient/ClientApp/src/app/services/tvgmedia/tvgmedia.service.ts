import { Observable } from "rxjs";
import {catchError, map } from 'rxjs/operators';
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import { HttpHeaders } from "@angular/common/http";
import { TvgMedia } from "../../types/media.type";
import { MatchTvgPostModel } from "../../types/matchTvgPostModel";
import { sitePackChannel } from "../../types/sitepackchannel.type";
import { environment } from "../../../environments/environment";

@Injectable({
  providedIn: 'root',
})
export class TvgMediaService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http);
    this._baseUrl = "tvgmedia";
  }

  get(id: string): Observable<ElasticResponse<TvgMedia>> {
    return this.http
      .get(environment.base_api_url + "tvgmedia/" + id).pipe(
      map(this.handleSuccess));
  }

  list(query: ElasticQuery): Observable<ElasticResponse<TvgMedia>> {
    return this.http
      .post(environment.base_api_url + "tvgmedia/_search/", query).pipe(
      map(this.handleSuccess));
  }

  addToToPlaylist(id: string, ...medias: TvgMedia[]) {
    return this.http
      .post(environment.base_api_url + `${this._baseUrl}/${id}/insert`, medias, {
        headers: new HttpHeaders().set("Content-Type", "application/json"),
        responseType: "text"
      });
  }

  removeFromPlaylist(id: string, ...medias: TvgMedia[]) {
    return this.http
      .post(`${environment.base_api_url}${this._baseUrl}/${id}/delete`, medias, {
        headers: new HttpHeaders().set("Content-Type", "application/json"),
        responseType: "text"
      });
  }

  matchTvg(matchTvgPostModel: MatchTvgPostModel): Observable<sitePackChannel> {
    return this.http
      .post(`${environment.base_api_url}${this._baseUrl}/matchtvg`, matchTvgPostModel).pipe(
      map(this.handleSuccess),
      );
  }
}
