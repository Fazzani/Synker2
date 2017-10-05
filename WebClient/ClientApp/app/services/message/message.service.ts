import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { ElasticQuery, ElasticResponse, PagedResult } from "../../types/elasticQuery.type";
import { Message } from "../../types/message.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';
import { RequestOptions } from "@angular/http/http";
import { HttpHeaders } from "@angular/common/http";

@Injectable()
export class MessageService extends BaseService {

    url: string = BaseService.URL_API_BASE + 'message/status/';

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