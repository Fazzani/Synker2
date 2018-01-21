import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import { map, catchError } from 'rxjs/operators';
import { RequestOptions } from "@angular/http/http";
import { HttpHeaders } from "@angular/common/http";
import { User } from '../../types/auth.type';

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
}