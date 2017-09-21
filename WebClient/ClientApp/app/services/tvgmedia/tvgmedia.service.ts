﻿import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { TvgMedia, ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';
import { RequestOptions } from "@angular/http/http";
import { HttpHeaders } from "@angular/common/http";

@Injectable()
export class TvgMediaService extends BaseService {

    constructor(protected http: HttpClient) { super(http); }

    get(id: string): Observable<ElasticResponse<TvgMedia>> {
        return this.http.get(BaseService.URL_API_BASE + 'tvgmedia/' + id).map(this.parseData)
            .catch(this.handleError);
    }

    list(query: ElasticQuery): Observable<ElasticResponse<TvgMedia>> {
        return this.http.post(BaseService.URL_API_BASE + 'tvgmedia/_search/', query).map(res => {
            return res;
        }).catch(this.handleError);
    }
}