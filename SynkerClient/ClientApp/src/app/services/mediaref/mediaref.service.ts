import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import {
  ElasticQuery,
  ElasticResponse,
  ElasticAggregations,
  SimpleQueryElastic
} from "../../types/elasticQuery.type";

// All the RxJS stuff we need
import { Observable } from "rxjs/Rx";
import { map } from "rxjs/operators";
import * as variables from "../../variables";
import { mediaRef } from "../../types/mediaref.type";
import { sitePackChannel } from "../../types/sitepackchannel.type";
import { environment } from "../../../environments/environment";

@Injectable()
export class MediaRefService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http, "mediasref");
  }

  get(id: string): Observable<ElasticResponse<mediaRef>> {
    return this.http
      .get(environment.base_api_url + `${this.BaseUrl}/${id}`)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }

  list(query: ElasticQuery): Observable<ElasticResponse<mediaRef>> {
    return this.http
      .post(environment.base_api_url + "mediasref/_search", query)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }

  synk(): Observable<ElasticResponse<mediaRef>> {
    return this.http
      .post(environment.base_api_url + "mediasref/synk", null)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }

  synkPicons(): Observable<ElasticResponse<mediaRef>> {
    return this.http
      .post(environment.base_api_url + "mediasref/synkpicons", null)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }

  save(...medias: mediaRef[]): Observable<ElasticResponse<mediaRef>> {
    return this.http
      .post(environment.base_api_url + this.BaseUrl, medias)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }

  /**
   * Get Group or filter by group name
   * @param {string} filter group name not required
   * @returns
   */
  groups(filter: string): Observable<ElasticAggregations> {
    return this.http
      .post(environment.base_api_url + "mediasref/groups", filter)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }

  /**
   * Get cultures or filter by culture name
   * @param {string} filter culture filter not required
   * @returns
   */
  cultures(filter?: string): Observable<string[]> {
    let f = filter ? filter : "_all";
    return this.http
      .get(environment.base_api_url + "mediasref/cultures?filter=" + f)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }

  /**
   * List tvg sites
   * @returns
   */
  tvgSites(): Observable<sitePackChannel[]> {
    return this.http
      .get<sitePackChannel[]>(environment.base_api_url + "sitepack/tvgsites")
      .map(
        res => {
          return res;
        },
        err => this.handleError(err)
      );
  }

  /**
   * List tvg sites
   * @returns
   */
  sitePacks(filter: string): Observable<sitePackChannel[]> {
    return this.http
      .get<sitePackChannel[]>(
        environment.base_api_url + "mediasref/sitepacks?filter=" + filter
      )
      .map(
        res => {
          return res;
        },
        err => this.handleError(err)
      );
  }
}
