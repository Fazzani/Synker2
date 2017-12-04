import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import {  ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import { map, catchError } from 'rxjs/operators';
import { RequestOptions } from "@angular/http/http";
import { HttpHeaders } from "@angular/common/http";
import * as variables from "../../variables";
import { TvgMedia } from '../../types/media.type';

@Injectable()
export class TvgMediaService extends BaseService {

    constructor(protected http: HttpClient) { super(http, 'tvgmedia'); }

    get(id: string): Observable<ElasticResponse<TvgMedia>> {
        return this.http.get(variables.BASE_API_URL + 'tvgmedia/' + id).map(this.parseData)
            .catch(this.handleError);
    }

    list(query: ElasticQuery): Observable<ElasticResponse<TvgMedia>> {
        return this.http.post(variables.BASE_API_URL + 'tvgmedia/_search/', query).map(res => {
            return res;
        }).catch(this.handleError);
    }
}