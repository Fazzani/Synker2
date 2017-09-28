import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { AuthResponse } from '../../types/auth.type';
// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';


@Injectable()
export class AuthService extends BaseService {

    constructor(protected http: HttpClient) {
        super(http);
    }

    Signin(userName: string, password: string): Observable<AuthResponse> {
        return this.http.post(BaseService.URL_API_BASE + 'token', { userName, password }).map(this.parseData)
            .catch(this.handleError);
    }
}