import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { ElasticQuery, ElasticResponse, ElasticAggregations, SimpleQueryElastic } from "../../types/elasticQuery.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import { map } from 'rxjs/operators';
import * as variables from '../../variables';
import { mediaRef } from '../../types/mediaref.type';
import { sitePackChannel } from '../../types/sitepackchannel.type';

@Injectable()
export class MediaRefService extends BaseService {

    constructor(protected http: HttpClient) {
        super(http, 'mediasref')
    }

    get(id: string): Observable<ElasticResponse<mediaRef>> {
      return this.http.get(variables.BASE_API_URL + `${this.BaseUrl}/${id}`).map(this.handleSuccess)
            .catch(this.handleError);
    }

    list(query: ElasticQuery): Observable<ElasticResponse<mediaRef>> {
      return this.http.post(variables.BASE_API_URL + 'mediasref/_search', query).map(this.handleSuccess).catch(this.handleError);
    }

    synk(): Observable<ElasticResponse<mediaRef>> {
      return this.http.post(variables.BASE_API_URL + 'mediasref/synk', null).map(this.handleSuccess).catch(this.handleError);
    }

    synkPicons(): Observable<ElasticResponse<mediaRef>> {
      return this.http.post(variables.BASE_API_URL + 'mediasref/synkpicons', null).map(this.handleSuccess).catch(this.handleError);
    }

    save(...medias: mediaRef[]): Observable<ElasticResponse<mediaRef>> {
      return this.http.post(variables.BASE_API_URL + this.BaseUrl, medias).map(this.handleSuccess).catch(this.handleError);
    }

    /**
     * Get Group or filter by group name
     * @param {string} filter group name not required 
     * @returns
     */
    groups(filter: string): Observable<ElasticAggregations> {
      return this.http.post(variables.BASE_API_URL + 'mediasref/groups', filter).map(this.handleSuccess).catch(this.handleError);
    }

    /**
    * Get cultures or filter by culture name
    * @param {string} filter culture filter not required
    * @returns
    */
    cultures(filter?: string): Observable<string[]> {
        let f = filter ? filter : "_all";
      return this.http.get(variables.BASE_API_URL + 'mediasref/cultures?filter=' + f).map(this.handleSuccess).catch(this.handleError);
    }

    /**
   * List tvg sites
   * @returns
   */
    tvgSites(): Observable<sitePackChannel[]> {
        return this.http.get<sitePackChannel[]>(variables.BASE_API_URL + 'sitepack/tvgsites').map(res => {
            return res;
        }, err => this.handleError(err));
    }


    /**
 * List tvg sites
 * @returns
 */
    sitePacks(filter: string): Observable<sitePackChannel[]> {
        return this.http.get<sitePackChannel[]>(variables.BASE_API_URL + 'mediasref/sitepacks?filter=' + filter).map(res => {
            return res;
        }, err => this.handleError(err));
    }
}
