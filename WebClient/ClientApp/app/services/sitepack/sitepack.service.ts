import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { ElasticQuery, ElasticResponse, ElasticAggregations, SimpleQueryElastic } from "../../types/elasticQuery.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import { map } from 'rxjs/operators';
import * as variables from '../../variables';
import { sitePackChannel } from '../../types/sitepackchannel.type';

@Injectable()
export class SitePackService extends BaseService {

    constructor(protected http: HttpClient) {
        super(http, 'sitepack')
    }

    get(id: string): Observable<ElasticResponse<sitePackChannel>> {
        return this.http.get(variables.BASE_API_URL + `${this.BaseUrl}/${id}`).map(this.parseData)
            .catch(this.handleError);
    }

    list(query: ElasticQuery): Observable<ElasticResponse<sitePackChannel>> {
        return this.http.post(variables.BASE_API_URL + 'sitepack/_search', query).map(res => {
            return res;
        }).catch(this.handleError);
    }

    save(...sitePacks: sitePackChannel[]): Observable<ElasticResponse<sitePackChannel>> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/save`, sitePacks).map(res => {
            return res;
        }).catch(this.handleError);
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
        return this.http.get<sitePackChannel[]>(variables.BASE_API_URL + 'sitepack/sitepacks?filter=' + filter).map(res => {
            return res;
        }, err => this.handleError(err));
    }
}