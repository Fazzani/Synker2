import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { ElasticQuery, ElasticResponse, ElasticAggregations, SimpleQueryElastic } from "../../types/elasticQuery.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import * as variables from '../../variables';
import { sitePackChannel } from '../../types/sitepackchannel.type';

@Injectable()
export class SitePackService extends BaseService {

    constructor(protected http: HttpClient) {
        super(http, 'sitepack')
    }

    get(id: string): Observable<ElasticResponse<sitePackChannel>> {
      return this.http.get(variables.BASE_API_URL + `${this.BaseUrl}/${id}`).map(this.handleSuccess)
            .catch(this.handleError);
    }

    list(query: ElasticQuery): Observable<ElasticResponse<sitePackChannel>> {
      return this.http.post(variables.BASE_API_URL + 'sitepack/_search', query).map(this.handleSuccess).catch(this.handleError);
    }

    save(...sitePacks: sitePackChannel[]): Observable<ElasticResponse<sitePackChannel>> {
      return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/save`, sitePacks).map(this.handleSuccess).catch(this.handleError);
    }

    /**
   * Get countries or filter by country name
   * @param {string} filter country filter not required
   * @returns
   */
    countries(filter?: string): Observable<string[]> {
        let f = filter ? filter : "_all";
      return this.http.get(`${variables.BASE_API_URL}${this.BaseUrl}/countries?filter=${f}`).map(this.handleSuccess).catch(this.handleError);
    }

    /**
     * * find tvg by mediaName and Site
     * @param {string} mediaName
     * @param {string} site?
     * @returns
     */
    matchTvgByMedia(mediaName: string, site?: string): Observable<sitePackChannel> {
      return this.http.get(`${variables.BASE_API_URL}${this.BaseUrl}/matchtvg/name/${mediaName}?site=${site}`)
        .map(this.handleSuccess)
        .catch(this.handleError);
    }

    /**
     * List tvg sites
     * @returns
     */
  tvgSites(): Observable<sitePackChannel[]> {
    return this.http.get(`${variables.BASE_API_URL}${this.BaseUrl}/tvgsites`).map(this.handleSuccess).catch(this.handleError);
    }

    /**
     * List tvg sites
     * @returns
     */
    sitePacks(filter: string): Observable<sitePackChannel[]> {
        return this.http.get<sitePackChannel[]>(`${variables.BASE_API_URL}${this.BaseUrl}/sitepacks?filter=${filter}`).map(res => {
            return res;
        }, err => this.handleError(err));
    }

    delete(id: string): Observable<number> {
      return this.http.delete(variables.BASE_API_URL + `${this.BaseUrl}?id=${id}`).map(this.handleSuccess).catch(this.handleError);
    }

    /**
     * Sync countries
     * @returns
     */
    syncCountries(): Observable<number> {
      return this.http.post(variables.BASE_API_URL + `${this.BaseUrl}/countries`, null).map(this.handleSuccess).catch(this.handleError);
    }

    /**
     * synk webgrab configuration from sitepack collected from playlists
     * @returns
     */
    synkWebgrab(): Observable<any> {
        return this.http.post(variables.BASE_API_URL + `${this.BaseUrl}/synk/webgrab`, null).map(res => {
            return res;
        }).catch(this.handleError);
    }
}
