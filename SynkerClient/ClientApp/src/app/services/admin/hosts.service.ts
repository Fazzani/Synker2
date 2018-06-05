import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';

// All the RxJS stuff we need
import { Observable } from 'rxjs/RX';
import { map, catchError } from 'rxjs/operators';
import { RequestOptions } from "@angular/http/http";
import { HttpHeaders } from "@angular/common/http";
import { PagedResult, QueryListBaseModel } from '../../types/common.type';
import { Host } from '../../types/host.type';

@Injectable()
export class HostsService extends BaseService {

    constructor(protected http: HttpClient) { super(http, 'host'); }

    update(host: Host): Observable<Host> {
      return this.http.put(`${this.FullBaseUrl}/${host.id}`, host)
        .map(this.handleSuccess)
            .catch(this.handleError);
    }

  public get(id: number): Observable<Host> {
    return this.http.get(`${this.FullBaseUrl}/${id}`)
      .map(this.handleSuccess)
            .catch(this.handleError);
    }

    public list(queryModel: QueryListBaseModel): Observable<PagedResult<Host>> {
      return this.http.post(`${this.FullBaseUrl}/search`, queryModel)
        .map(this.handleSuccess)
            .catch(this.handleError);
  }

  
}
