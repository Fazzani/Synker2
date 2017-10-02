﻿import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { AuthResponse, User, RegisterUser } from '../../types/auth.type';
// All the RxJS stuff we need
import { JwtHelper, tokenNotExpired } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/switchMap';



@Injectable()
export class AuthService extends BaseService {

    /**
     * Is user authenticated
     * 
     * @type {BehaviorSubject<boolean>}
     * @memberof AuthService
     */
    authenticated: BehaviorSubject<boolean>;
    /**
     * User info
     * 
     * @type {BehaviorSubject<User>}
     * @memberof AuthService
     */
    user: BehaviorSubject<User>;

    private TOKEN_ENDPOINT: string;
    private REVOCATION_ENDPOINT: string;
    REGISTER_ENDPONT: string;
    /**
     * Stores the URL so we can redirect after signing in. 
     * 
     * @type {string}
     * @memberof AuthService
     */
    public redirectUrl: string;

    /**
     * User's data. 
     * @param {HttpClient} http 
     * @param {Router} router 
     * @memberof AuthService
     */
    constructor(protected http: HttpClient, private router: Router) {
        super(http);
        this.REGISTER_ENDPONT = BaseService.URL_API_BASE + 'auth/register';
        this.TOKEN_ENDPOINT = BaseService.URL_API_BASE + 'auth/token';
        this.REVOCATION_ENDPOINT = BaseService.URL_API_BASE + 'auth/revoketoken';
        this.authenticated = new BehaviorSubject(false);
        this.user = new BehaviorSubject(null);
    }

    /**
     * Get Token from storage
     * 
     * @returns {string} 
     * @memberof AuthService
     */
    public getToken(): string {
        // get the token
        return localStorage.getItem('accessToken');
    }

    /**
     * Update Authenticated field
     * 
     * @memberof AuthService
     */
    public isAuthenticated(): void {
        // return a boolean reflecting 
        // whether or not the token is expired
        let res = tokenNotExpired('accessToken');
        if (!res) {
            //Try to refresh it
            this.getNewToken();
            res = tokenNotExpired('accessToken');
        }
        this.decodeToken();
    }

    /**
     *  Sigin by password
     * store access and refresh token
     * 
     * @param {string} username 
     * @param {string} password 
     * @returns {Observable<AuthResponse>} 
     * @memberof AuthService
     */
    public Signin(username: string, password: string): Observable<AuthResponse> {
        return this.http.post(this.TOKEN_ENDPOINT, { username, password })
            .map((res: AuthResponse) => {

                // Sign in successful if there's an access token in the response.  
                if (typeof res.accessToken !== 'undefined') {

                    // Stores access token & refresh token.  
                    this.store(res);
                    this.router.navigateByUrl(this.redirectUrl);
                }
                return res;
            });
    }

    /**
     * Register user
     * 
     * @param {RegisterUser} user 
     * @returns {Observable<any>} 
     * @memberof AuthService
     */
    public Register(user: RegisterUser): Observable<any> {
        return this.http.post(this.REGISTER_ENDPONT, user)
            .switchMap((res: any) => {
                return this.Signin(user.username, user.password);
            });
    }

    /**
     * Tries to get a new token using refresh token. 
     * 
     * @memberof AuthService
     */
    public getNewToken(): void {

        let refreshToken: string = localStorage.getItem('refreshToken');

        if (refreshToken != null) {

            // Token endpoint & params.  
            let tokenEndpoint: string = this.TOKEN_ENDPOINT;

            let params: any = {
                grantType: 1,
                refreshToken: refreshToken
            };

            this.http.post(tokenEndpoint, params)
                .subscribe(
                (res: AuthResponse) => {

                    // Successful if there's an access token in the response.  
                    if (typeof res.accessToken !== 'undefined') {
                        this.decodeToken();
                        // Stores access token & refresh token.  
                        this.store(res);
                    }

                });
        }
    }

    /**
     * Revokes token. 
     * 
     * @memberof AuthService
     */
    public revokeToken(): void {

        let token: string = this.getToken();
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
     * 
     * @memberof AuthService
     */
    public revokeRefreshToken(): void {

        let refreshToken: string = localStorage.getItem('refreshToken');

        if (refreshToken != null) {

            // Revocation endpoint & params.  
            let revocationEndpoint: string = this.REVOCATION_ENDPOINT;

            let params: any = {
                token: refreshToken
            };

            this.http.post(revocationEndpoint, params)
                .subscribe(
                () => {

                    localStorage.removeItem('refreshToken');

                }, err => localStorage.removeItem('refreshToken'));
        }
    }

    /**
     *  Removes user and revokes tokens. 
     * 
     * @memberof AuthService
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
     *  Get authenticated user
     * 
     * @memberof AuthService
     */
    public getUser(): void {
        this.decodeToken();
    }

    /**
     *  Decodes token through JwtHelper.
     * 
     * @private
     * @memberof AuthService
     */
    private decodeToken(): void {

        if (tokenNotExpired('accessToken')) {

            let token: string = this.getToken();

            let jwtHelper: JwtHelper = new JwtHelper();
            this.authenticated.next(true);
            this.user.next(jwtHelper.decodeToken(token));
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
}