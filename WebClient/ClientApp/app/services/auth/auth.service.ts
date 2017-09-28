import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';


@Injectable()
export class AuthService extends BaseService {

    constructor(protected http: HttpClient) {
        super(http);
    }

    get(id: string): Observable<boolean> {
        return this.http.get(BaseService.URL_API_BASE + 'epg/' + id).map(this.parseData)
            .catch(this.handleError);
    }

    list(query: ElasticQuery): Observable<ElasticResponse<tvChannel>> {
        return this.http.post(BaseService.URL_API_BASE + 'epg/_search/', query).map(res => {
            return res;
        }).catch(this.handleError);
    }

}