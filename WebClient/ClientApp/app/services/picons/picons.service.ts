import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { ElasticQuery, ElasticResponse, ElasticAggregations } from "../../types/elasticQuery.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import { map } from 'rxjs/operators';
import * as variables from '../../variables';
import { picon } from '../../types/picon.type';

@Injectable()
export class PiconService extends BaseService {
    BaseUrl: string = "picons";

    constructor(protected http: HttpClient) {
        super(http);
    }

    get(id: string): Observable<ElasticResponse<picon>> {
        return this.http.get(`${variables.BASE_API_URL}${this.BaseUrl}/${id}`).map(this.parseData)
            .catch(this.handleError);
    }

    list(field: string, filter: string): Observable<ElasticResponse<picon>> {
        let q = ElasticQuery.Match(field, filter);
        
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/_search`, q).map(res => {
            return res;
        }).catch(this.handleError);
    }

    delete(id: string): Observable<number> {
        return this.http.delete(`${variables.BASE_API_URL}${this.BaseUrl}/${id}`).map(res => {
            return res;
        }).catch(this.handleError);
    }

    synk(): Observable<ElasticResponse<picon>> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/synk`, null).map(res => {
            return res;
        }).catch(this.handleError);
    }
}