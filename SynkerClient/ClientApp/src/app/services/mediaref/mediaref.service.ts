import { Observable } from "rxjs";
import { map } from 'rxjs/operators';
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import { ElasticQuery, ElasticResponse, ElasticAggregations } from "../../types/elasticQuery.type";
import { mediaRef } from "../../types/mediaref.type";
import { sitePackChannel } from "../../types/sitepackchannel.type";
import { environment } from "../../../environments/environment";

@Injectable({
  providedIn: 'root',
})
export class MediaRefService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http);
    this._baseUrl = "mediasref";
  }

  get(id: string): Observable<ElasticResponse<mediaRef>> {
    return this.http
      .get(environment.base_api_url + `${this._baseUrl}/${id}`).pipe(
        map(this.handleSuccess));
  }

  list(query: ElasticQuery): Observable<ElasticResponse<mediaRef>> {
    return this.http
      .post(environment.base_api_url + "mediasref/_search", query).pipe(
        map(this.handleSuccess));
  }

  synk(): Observable<ElasticResponse<mediaRef>> {
    return this.http
      .post(environment.base_api_url + "mediasref/synk", null).pipe(
        map(this.handleSuccess));
  }

  synkPicons(): Observable<ElasticResponse<mediaRef>> {
    return this.http
      .post(environment.base_api_url + "mediasref/synkpicons", null).pipe(
        map(this.handleSuccess));
  }

  save(...medias: mediaRef[]): Observable<ElasticResponse<mediaRef>> {
    return this.http
      .post(environment.base_api_url + this._baseUrl, medias).pipe(
        map(this.handleSuccess));
  }

  /**
   * Get Group or filter by group name
   * @param {string} filter group name not required
   * @returns
   */
  groups(filter: string): Observable<ElasticAggregations> {
    return this.http
      .post(environment.base_api_url + "mediasref/groups", filter).pipe(
        map(this.handleSuccess));
  }

  /**
   * Get cultures or filter by culture name
   * @param {string} filter culture filter not required
   * @returns
   */
  cultures(filter?: string): Observable<string[]> {
    let f = filter ? filter : "_all";
    return this.http
      .get(environment.base_api_url + "mediasref/cultures?filter=" + f).pipe(
        map(this.handleSuccess));
  }

  /**
   * List tvg sites
   * @returns
   */
  tvgSites(): Observable<sitePackChannel[]> {
    return this.http.get<sitePackChannel[]>(environment.base_api_url + "sitepack/tvgsites");
  }

  /**
   * List tvg sites
   * @returns
   */
  sitePacks(filter: string): Observable<sitePackChannel[]> {
    return this.http.get<sitePackChannel[]>(environment.base_api_url + "mediasref/sitepacks?filter=" + filter);
  }
}
