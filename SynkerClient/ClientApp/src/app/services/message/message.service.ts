import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import { Message, MessageQueryModel } from "../../types/message.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs/RX';
import { map, catchError } from 'rxjs/operators';
import { RequestOptions } from "@angular/http/http";
import { HttpHeaders } from "@angular/common/http";
import * as variables from "../../variables";
import { PagedResult } from '../../types/common.type';

@Injectable()
export class MessageService extends BaseService {

    url: string = `${this.FullBaseUrl}/status/`;

    constructor(protected http: HttpClient) { super(http, 'message'); }

    public get(id: string): Observable<PagedResult<Message>> {
      return this.http.get(this.url + id).map(this.handleSuccess)
            .catch(this.handleError);
    }

    public update(message: Message): Observable<number> {
      return this.http.put(`${this.FullBaseUrl}/${message.id}`, message).map(this.handleSuccess)
            .catch(this.handleError);
    }

    public list(): Observable<PagedResult<Message>> {
      return this.http.get(this.url).map(this.handleSuccess)
            .catch(this.handleError);
    }

    public listByStatus(status: number[], page: number, pageSize: number): Observable<PagedResult<Message>> {
        return this.http.post(`${this.FullBaseUrl}/search/status`, <MessageQueryModel>{ MessageStatus: status, pageNumber: page, pageSize: pageSize })
          .map(this.handleSuccess).catch(this.handleError);
    }
}
