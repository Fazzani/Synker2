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

    private TOKEN_ENDPOINT: string;
    private REVOCATION_ENDPOINT: string;
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
        this.TOKEN_ENDPOINT = BaseService.URL_API_BASE + 'auth/token';
        this.REVOCATION_ENDPOINT = BaseService.URL_API_BASE + 'auth/revoketoken';
    }

    public getToken(): string {
        // get the token
        return localStorage.getItem('accessToken');
    }


    public isAuthenticated(): boolean {
        // return a boolean reflecting 
        // whether or not the token is expired
        var res = tokenNotExpired('accessToken');
        return res;
    }

    /**
     * Sigin by password
     * store access and refresh token
     * @param username
     * @param password
     */
    public Signin(username: string, password: string): Observable<AuthResponse> {
        return this.http.post(this.TOKEN_ENDPOINT, { username, password })
            .map((res: AuthResponse) => {

                // Sign in successful if there's an access token in the response.  
                if (typeof res.accessToken !== 'undefined') {

                    // Stores access token & refresh token.  
                    this.store(res);

                }
                return res;
            });
    }

    /** 
     * Tries to get a new token using refresh token. 
     */
    public getNewToken(): void {

        let refreshToken: string = this.getToken();

        if (refreshToken != null) {

            // Token endpoint & params.  
            let tokenEndpoint: string = this.TOKEN_ENDPOINT;

            let params: any = {
                grant_type: 1,
                refresh_token: refreshToken
            };

            // Encodes the parameters.  
            let body: string = this.encodeParams(params);

            this.http.post(tokenEndpoint, body)
                .subscribe(
                (res: Response) => {

                    let body: any = res.json();

                    // Successful if there's an access token in the response.  
                    if (typeof body.access_token !== 'undefined') {

                        // Stores access token & refresh token.  
                        this.store(body);

                    }

                });
        }
    }

    /** 
     * Revokes token. 
     */
    public revokeToken(): void {

        let token: string = this.getToken();
        debugger;
        if (token != null) {

            // Revocation endpoint & params.  
            let revocationEndpoint: string = this.REVOCATION_ENDPOINT;

            this.http.post(revocationEndpoint, { token: token })
                .subscribe(
                () => {
                    localStorage.removeItem('accessToken');
                }, err => localStorage.removeItem('accessToken'));
        }
    }

    /** 
     * Revokes refresh token. 
     */
    public revokeRefreshToken(): void {

        let refreshToken: string = localStorage.getItem('refreshToken');

        if (refreshToken != null) {

            // Revocation endpoint & params.  
            let revocationEndpoint: string = this.REVOCATION_ENDPOINT;

            let params: any = {
                token: refreshToken
            };

            // Encodes the parameters.  
            let body: string = this.encodeParams(params);

            this.http.post(revocationEndpoint, body)
                .subscribe(
                () => {

                    localStorage.removeItem('refreshToken');

                }, err => localStorage.removeItem('refreshToken'));
        }
    }

    /** 
     * Removes user and revokes tokens. 
     */
    public signout(): void {

        this.redirectUrl = null;

        this.user = null;

        // Revokes token.  
        this.revokeToken();

        // Revokes refresh token.  
        this.revokeRefreshToken();

    }

    /**
     * Get authenticated user
     */
    public getUser(): User {
        if (!this.user)
            this.decodeToken();
        return this.user;
    }
    /** 
     * Decodes token through JwtHelper. 
     */
    private decodeToken(): void {

        if (tokenNotExpired('accessToken')) {

            let token: string = this.getToken();

            let jwtHelper: JwtHelper = new JwtHelper();
            this.user = jwtHelper.decodeToken(token);

        }

    }
    /** 
     * Stores access token & refresh token. 
     * 
     * @param body The response of the request to the token endpoint 
     */
    private store(body: AuthResponse): void {

        // Stores access token in local storage to keep user signed in.  
        localStorage.setItem('accessToken', body.accessToken);
        // Stores refresh token in local storage.  
        localStorage.setItem('refreshToken', body.refreshToken);

        // Decodes the token.  
        this.decodeToken();

    }

    /** 
     * // Encodes the parameters. 
     * 
     * @param params The parameters to be encoded 
     * @return The encoded parameters 
     */
    private encodeParams(params: any): string {

        let body: string = "";
        for (let key in params) {
            if (body.length) {
                body += "&";
            }
            body += key + "=";
            body += encodeURIComponent(params[key]);
        }

        return body;
    }
}