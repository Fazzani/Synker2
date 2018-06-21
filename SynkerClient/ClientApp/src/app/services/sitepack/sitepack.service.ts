
import {catchError, map  } from 'rxjs/operators';
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";

// All the RxJS stuff we need
import { Observable } from "rxjs";
import { sitePackChannel } from "../../types/sitepackchannel.type";
import { environment } from "../../../environments/environment";

@Injectable({
  providedIn: 'root',
})
export class SitePackService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http);
    this._baseUrl = "sitepack";
  }

  get(id: string): Observable<ElasticResponse<sitePackChannel>> {
    return this.http
      .get(environment.base_api_url + `${this._baseUrl}/${id}`).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  list(query: ElasticQuery): Observable<ElasticResponse<sitePackChannel>> {
    return this.http
      .post(environment.base_api_url + "sitepack/_search", query).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  save(...sitePacks: sitePackChannel[]): Observable<ElasticResponse<sitePackChannel>> {
    return this.http
      .post(`${environment.base_api_url}${this._baseUrl}/save`, sitePacks).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  /**
   * Get countries or filter by country name
   * @param {string} filter country filter not required
   * @returns
   */
  countries(filter?: string): Observable<string[]> {
    let f = filter ? filter : "_all";
    return this.http
      .get(`${environment.base_api_url}${this._baseUrl}/countries?filter=${f}`).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  /**
   * * find tvg by mediaName and Site
   * @param {string} mediaName
   * @param {string} site?
   * @returns
   */
  matchTvgByMedia(mediaName: string, site?: string): Observable<sitePackChannel> {
    return this.http
      .get(`${environment.base_api_url}${this._baseUrl}/matchtvg/name/${mediaName}?site=${site}`).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  /**
   * List tvg sites
   * @returns
   */
  tvgSites(): Observable<sitePackChannel[]> {
    return this.http
      .get(`${environment.base_api_url}${this._baseUrl}/tvgsites`).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  /**
   * List tvg sites
   * @returns
   */
  sitePacks(filter: string): Observable<sitePackChannel[]> {
    return this.http.get<sitePackChannel[]>(`${environment.base_api_url}${this._baseUrl}/sitepacks?filter=${filter}`).pipe(map(
      res => {
        return res;
      },
      err => this.handleError(err)
    ));
  }

  delete(id: string): Observable<number> {
    return this.http
      .delete(environment.base_api_url + `${this._baseUrl}?id=${id}`).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  /**
   * Sync countries
   * @returns
   */
  syncCountries(): Observable<number> {
    return this.http
      .post(environment.base_api_url + `${this._baseUrl}/countries`, null).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  /**
   * synk webgrab configuration from sitepack collected from playlists
   * @returns
   */
  synkWebgrab(): Observable<any> {
    return this.http
      .post(environment.base_api_url + `${this._baseUrl}/synk/webgrab`, null).pipe(
      map(res => {
        return res;
      }),
      catchError(this.handleError),);
  }
}
