import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import { tvChannel } from "../../types/xmltv.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import { map, catchError } from 'rxjs/operators';
import * as variables from '../../variables';

@Injectable()
export class EpgService extends BaseService {

    constructor(protected http: HttpClient) {
        super(http,'epg');
    }

    get(id: string): Observable<ElasticResponse<tvChannel>> {
        return this.http.get(variables.BASE_API_URL + `${this.BaseUrl}/${id}`).map(this.parseData)
            .catch(this.handleError);
    }

    list(query: ElasticQuery): Observable<ElasticResponse<tvChannel>> {
        return this.http.post(variables.BASE_API_URL + 'epg/_search/', query).map(res => {
            return res;
        }).catch(this.handleError);
    }

}