import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { AuthResponse, User } from '../../types/auth.type';
// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

import { JwtHelper, tokenNotExpired } from 'angular2-jwt'; 

@Injectable()
export class AuthService extends BaseService {

    /** 
     * Stores the URL so we can redirect after signing in. 
     */
    public redirectUrl: string; 
    
    /**
    * User's data. 
    */  
    private user: User;  

    constructor(protected http: HttpClient) {
        super(http);
    }


    /**
     * Sigin by password
     * store access and refresh token
     * @param userName
     * @param password
     */
    public Signin(userName: string, password: string): Observable<AuthResponse> {
        return this.http.post(BaseService.URL_API_BASE + 'token', { userName, password })
            .map((res: AuthResponse) => {

                // Sign in successful if there's an access token in the response.  
                if (typeof res.accessToken !== 'undefined') {

                    // Stores access token & refresh token.  
                    this.store(res);
                }

            })
            .catch(this.handleError);
    }

    /** 
     * Removes user and revokes tokens. 
     */
    public signout(): void {

        this.redirectUrl = null;

        this.user = null;

        //// Revokes token.  
        //this.revokeToken();

        //// Revokes refresh token.  
        //this.revokeRefreshToken();

    }  

    /** 
     * Decodes token through JwtHelper. 
     */
    private decodeToken(): void {

        if (tokenNotExpired()) {

            let token: string = localStorage.getItem('accessToken');

            let jwtHelper: JwtHelper = new JwtHelper();
            this.user = jwtHelper.decodeToken(token);

        }

    }  
    /** 
     * Stores access token & refresh token. 
     * 
     * @param body The response of the request to the token endpoint 
     */
    private store(body: any): void {

        // Stores access token in local storage to keep user signed in.  
        localStorage.setItem('accessToken', body.access_token);
        // Stores refresh token in local storage.  
        localStorage.setItem('refreshToken', body.refresh_token);

        // Decodes the token.  
        this.decodeToken();

    }  
}