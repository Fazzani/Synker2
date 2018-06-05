import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import { tvChannel } from "../../types/xmltv.type";

// All the RxJS stuff we need
import { Observable } from 'rxjs/RX';
import { map, catchError } from 'rxjs/operators';
import * as variables from '../../variables';
import { environment } from '../../../environments/environment';

@Injectable()
export class EpgService extends BaseService {

    constructor(protected http: HttpClient) {
        super(http,'epg');
    }

    get(id: string): Observable<ElasticResponse<tvChannel>> {
      return this.http.get(environment.base_api_url + `${this.BaseUrl}/${id}`).map(this.handleSuccess)
            .catch(this.handleError);
    }

    list(query: ElasticQuery): Observable<ElasticResponse<tvChannel>> {
      return this.http.post(environment.base_api_url + 'epg/_search/', query).map(this.handleSuccess).catch(this.handleError);
    }

}
