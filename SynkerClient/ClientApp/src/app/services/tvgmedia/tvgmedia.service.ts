import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import {  ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs/RX';
import { map, catchError } from 'rxjs/operators';
import { RequestOptions } from "@angular/http/http";
import { HttpHeaders } from "@angular/common/http";
import * as variables from "../../variables";
import { TvgMedia } from '../../types/media.type';
import { MatchTvgPostModel } from '../../types/matchTvgPostModel';
import { sitePackChannel } from '../../types/sitepackchannel.type';
import { environment } from '../../../environments/environment';

@Injectable()
export class TvgMediaService extends BaseService {

    constructor(protected http: HttpClient) { super(http, 'tvgmedia'); }

    get(id: string): Observable<ElasticResponse<TvgMedia>> {
      return this.http.get(environment.base_api_url + 'tvgmedia/' + id).map(this.handleSuccess)
            .catch(this.handleError);
    }

    list(query: ElasticQuery): Observable<ElasticResponse<TvgMedia>> {
      return this.http.post(environment.base_api_url + 'tvgmedia/_search/', query).map(this.handleSuccess).catch(this.handleError);
    }

    addToToPlaylist(id: string, ...medias: TvgMedia[]) {
        return this.http.post(environment.base_api_url + `${this.BaseUrl}/${id}/insert`, medias,
            { headers: new HttpHeaders().set('Content-Type', 'application/json'), responseType: 'text' }).catch(this.handleError);
    }

    removeFromPlaylist(id: string, ...medias: TvgMedia[]) {
        return this.http.post(`${environment.base_api_url}${this.BaseUrl}/${id}/delete`, medias,
            { headers: new HttpHeaders().set('Content-Type', 'application/json'), responseType: 'text' }).catch(this.handleError);
    }

    matchTvg(matchTvgPostModel: MatchTvgPostModel) : Observable<sitePackChannel>{
      return this.http.post(`${environment.base_api_url}${this.BaseUrl}/matchtvg`, matchTvgPostModel).map(this.handleSuccess).catch(this.handleError);
    }

}
