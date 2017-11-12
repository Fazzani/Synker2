import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { ElasticQuery, ElasticResponse, ElasticAggregations } from "../../types/elasticQuery.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import { map } from 'rxjs/operators';
import * as variables from '../../variables';
import { mediaRef } from '../../types/mediaref.type';

@Injectable()
export class MediaRefService extends BaseService {

    constructor(protected http: HttpClient) {
        super(http);
    }

    get(id: string): Observable<ElasticResponse<mediaRef>> {
        return this.http.get(variables.BASE_API_URL + 'mediasref/' + id).map(this.parseData)
            .catch(this.handleError);
    }

    list(query: ElasticQuery): Observable<ElasticResponse<mediaRef>> {
        return this.http.post(variables.BASE_API_URL + 'mediasref/_search', query).map(res => {
            return res;
        }).catch(this.handleError);
    }

    synk(): Observable<ElasticResponse<mediaRef>> {
        return this.http.post(variables.BASE_API_URL + 'mediasref/synk', null).map(res => {
            return res;
        }).catch(this.handleError);
    }

    save(medias: Array<mediaRef>): Observable<ElasticResponse<mediaRef>> {
        return this.http.post(variables.BASE_API_URL + 'mediasref/save', medias).map(res => {
            return res;
        }).catch(this.handleError);
    }
    /**
     * Get Group or filter by group name
     * @param {string} filter group name not required 
     * @returns
     */
    groups(filter: string): Observable<ElasticAggregations> {
        return this.http.post(variables.BASE_API_URL + 'mediasref/groups', filter).map(res => {
            return res;
            }).catch(this.handleError);
    }

}