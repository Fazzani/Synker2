import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import { Message } from "../../types/message.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';
import { RequestOptions } from "@angular/http/http";
import { HttpHeaders } from "@angular/common/http";

@Injectable()
export class MessageService extends BaseService {

    url: string = BaseService.URL_API_BASE + 'message/';

    constructor(protected http: HttpClient) { super(http); }

    get(id: string): Observable<ElasticResponse<Message>> {
        return this.http.get(this.url + id).map(this.parseData)
            .catch(this.handleError);
    }

    list(): Observable<ElasticResponse<Message>> {
        return this.http.get(this.url).map(res => {
            return res;
        }).catch(this.handleError);
    }
}