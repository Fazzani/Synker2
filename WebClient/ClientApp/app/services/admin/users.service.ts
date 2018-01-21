import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import { map, catchError } from 'rxjs/operators';
import { RequestOptions } from "@angular/http/http";
import { HttpHeaders } from "@angular/common/http";
import { User } from '../../types/auth.type';
import { PagedResult, QueryListBaseModel } from '../../types/common.type';

@Injectable()
export class UsersService extends BaseService {

    constructor(protected http: HttpClient) { super(http, 'users'); }

    update(user: User): Observable<User> {
        return this.http.put(`${this.FullBaseUrl}/${user.id}`, user)
            .catch(this.handleError);
    }

    public me(): Observable<User> {
        return this.http.get(`${this.FullBaseUrl}`)
            .catch(this.handleError);
    }

    public get(id: number): Observable<User> {
        return this.http.get(`${this.FullBaseUrl}/${id}`)
            .catch(this.handleError);
    }

    public list(queryModel : QueryListBaseModel): Observable<PagedResult<User>> {
        return this.http.post(`${this.FullBaseUrl}/search`, queryModel)
            .catch(this.handleError);
    }
}