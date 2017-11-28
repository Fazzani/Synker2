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

    constructor(protected http: HttpClient) {
        super(http);
    }

    get(id: string): Observable<ElasticResponse<picon>> {
        return this.http.get(variables.BASE_API_URL + 'picons/' + id).map(this.parseData)
            .catch(this.handleError);
    }

    list(field: string, filter: string): Observable<ElasticResponse<picon>> {
        let q = ElasticQuery.Match(field, filter);
        console.log("list picons ", q);
        return this.http.post(variables.BASE_API_URL + 'picons/_search', q).map(res => {
            return res;
        }).catch(this.handleError);
    }

    delete(id: string): Observable<number> {
        return this.http.delete(variables.BASE_API_URL + 'picons/' + id).map(res => {
            return res;
        }).catch(this.handleError);
    }
}