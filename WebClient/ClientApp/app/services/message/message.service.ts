import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import { Message } from "../../types/message.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import { map, catchError } from 'rxjs/operators';
import { RequestOptions } from "@angular/http/http";
import { HttpHeaders } from "@angular/common/http";
import * as variables from "../../variables";
import { PagedResult } from '../../types/common.type';

@Injectable()
export class MessageService extends BaseService {

    url: string = variables.BASE_API_URL + 'message/status/';

    constructor(protected http: HttpClient) { super(http); }

    public get(id: string): Observable<PagedResult<Message>> {
        return this.http.get(this.url + id)
            .catch(this.handleError);
    }

    public list(): Observable<PagedResult<Message>> {
        return this.http.get(this.url)
            .catch(this.handleError);
    }

    public listByStatus(status: number, page: number, pageSize: number): Observable<PagedResult<Message>> {
        return this.http.get(`${this.url}${status}/${page}/${pageSize}`)
            .catch(this.handleError);
    }
}