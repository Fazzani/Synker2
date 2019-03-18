//import { throwError as observableThrowError, of as observableOf, Observable, BehaviorSubject } from 'rxjs';
//import { switchMap, map } from 'rxjs/operators';
//import { Injectable } from "@angular/core";
//import { Router } from "@angular/router";
//import { HttpClient } from "@angular/common/http";
//import { BaseService } from "../base/base.service";
//import { AuthResponse, User, RegisterUser, Login, AuthModel } from "../../types/auth.type";
//import { environment } from "../../../environments/environment";
//import { JwtHelperService } from '@auth0/angular-jwt';

//@Injectable({
//  providedIn: 'root',
//})
//export class AuthService extends BaseService {
//  /**
//   * Is user authenticated
//   *
//   * @type {BehaviorSubject<boolean>}
//   * @memberof AuthService
//   */
//  authenticated: BehaviorSubject<boolean>;
//  /**
//   * User info
//   *
//   * @type {BehaviorSubject<User>}
//   * @memberof AuthService
//   */
//  user: BehaviorSubject<User>;

//  private TOKEN_ENDPOINT: string;
//  private REVOCATION_ENDPOINT: string;
//  REGISTER_ENDPONT: string;
//  /**
//   * Stores the URL so we can redirect after signing in.
//   *
//   * @type {string}
//   * @memberof AuthService
//   */
//  public redirectUrl: string;

//  /**
//   * User's data.
//   * @param {HttpClient} http
//   * @param {Router} router
//   * @memberof AuthService
//   */
//  constructor(protected http: HttpClient, private router: Router, private jwtHelper: JwtHelperService) {
//    super(http);
//    this._baseUrl = "auth";
//    this.REGISTER_ENDPONT = environment.base_api_url + "auth/register";
//    this.TOKEN_ENDPOINT = environment.base_api_url + "auth/token";
//    this.REVOCATION_ENDPOINT = environment.base_api_url + "auth/revoketoken";
//    this.authenticated = new BehaviorSubject(false);
//    this.user = new BehaviorSubject(null);
//  }

//  /**
//   * Get Token from storage
//   *
//   * @returns {string}
//   * @memberof AuthService
//   */
//  public getToken(): string {
//    // get the token
//    return localStorage.getItem("access_token");
//  }

//  public isAuthorized(role: string): Observable<boolean> {
//    let accessToken: string | null = this.getToken();
//    if (accessToken == null) return observableOf(false);
//    return this.ConvertTokenToUser(accessToken).pipe(map(user => {
//      if (user.roles == undefined) return false;
//      return user.roles.indexOf(role) != -1;
//    }));
//  }
//  /**
//   * Update Authenticated field
//   *
//   * @memberof AuthService
//   */
//  public isAuthenticated(): Observable<boolean> {
//    let res = this.jwtHelper.isTokenExpired(this.getToken());
//    if (res) {
//      console.log("Try to refresh it");
//      this.getNewToken();
//      res = this.jwtHelper.isTokenExpired(this.getToken());
//    }

//    this.decodeToken();
//    return observableOf(true);
//  }

//  /**
//   *  Sigin by password
//   * store access and refresh token
//   * @param loginModel
//   * @memberof AuthService
//   */
//  public Signin(loginModel: Login): Observable<AuthResponse> {
//    return this.http.post(this.TOKEN_ENDPOINT, loginModel).pipe(map((res: AuthResponse) => {
//      // Sign in successful if there's an access token in the response.
//      if (typeof res.accessToken !== "undefined") {
//        console.log(`Stores access token & refresh token.  `);
//        this.store(res);
//        if (this.redirectUrl == undefined) this.redirectUrl = "/";
//        console.log(`navigateByUrl ${this.redirectUrl}`);
//        this.router.navigateByUrl(this.redirectUrl);
//      }
//      return res;
//    }));
//  }

//  /**
//   * Register user
//   *
//   * @param {RegisterUser} user
//   * @returns {Observable<any>}
//   * @memberof AuthService
//   */
//  public Register(user: RegisterUser): Observable<any> {
//    return this.http.post(this.REGISTER_ENDPONT, user).pipe(switchMap((res: any) => {
//      return this.Signin(<Login>{ username: user.username, password: user.password });
//    }));
//  }

//  /**
//   * Tries to get a new token using refresh token.
//   *
//   * @memberof AuthService
//   */
//  public getNewToken(): void {
//    let refreshToken: string | null = localStorage.getItem("refreshToken");
//    let accessToken: string | null = localStorage.getItem("access_token");

//    if (refreshToken != null && accessToken != null) {
//      // Token endpoint & params.
//      let tokenEndpoint: string = this.TOKEN_ENDPOINT;
//      this.ConvertTokenToUser(accessToken).subscribe(
//        user => {
//          if (user != null) {
//            let params: AuthModel = <AuthModel>{
//              grantType: 1,
//              refreshToken: refreshToken,
//              userName: user.email
//            };

//            this.http.post(tokenEndpoint, params).subscribe((res: AuthResponse) => {
//              // Successful if there's an access token in the response.
//              if (typeof res.accessToken !== "undefined") {
//                this.decodeToken();
//                // Stores access token & refresh token.
//                this.store(res);
//              }
//            });
//          }
//        },
//        err => console.log(err)
//      );
//    }
//  }

//  /**
//   * Revokes token.
//   *
//   * @memberof AuthService
//   */
//  public revokeToken(): void {
//    let token: string = this.getToken();
//    if (token != null) {
//      // Revocation endpoint & params.
//      let revocationEndpoint: string = this.REVOCATION_ENDPOINT;

//      this.http.post(revocationEndpoint, { token: token }).subscribe(
//        () => {
//          localStorage.removeItem("access_token");
//        },
//        err => localStorage.removeItem("access_token")
//      );
//    }
//  }

//  /**
//   * Revokes refresh token.
//   *
//   * @memberof AuthService
//   */
//  public revokeRefreshToken(): void {
//    let refreshToken: string = localStorage.getItem("refreshToken");

//    if (refreshToken != null) {
//      // Revocation endpoint & params.
//      let revocationEndpoint: string = this.REVOCATION_ENDPOINT;

//      let params: any = {
//        token: refreshToken
//      };

//      this.http.post(revocationEndpoint, params).subscribe(
//        () => {
//          localStorage.removeItem("refreshToken");
//        },
//        err => localStorage.removeItem("refreshToken")
//      );
//    }
//  }

//  /**
//   *  Removes user and revokes tokens.
//   *
//   * @memberof AuthService
//   */
//  public signout(): void {
//    this.redirectUrl = null;

//    this.user = new BehaviorSubject(null);

//    // Revokes token.
//    this.revokeToken();

//    // Revokes refresh token.
//    this.revokeRefreshToken();
//  }

//  /**
//   *  Get authenticated user
//   *
//   * @memberof AuthService
//   */
//  public connect(): void {
//    this.decodeToken();
//  }

//  /**
//   *  Decodes token through JwtHelper.
//   *
//   * @private
//   * @memberof AuthService
//   */
//  private decodeToken(): void {
//    if (!this.jwtHelper.isTokenExpired(this.getToken())) {
//      let token: string = this.getToken();

//      //let jwtHelper: JwtHelper = new JwtHelper();
//      this.authenticated.next(true);
//      this.user.next(this.mapTokenToUserModel(this.jwtHelper.decodeToken(token)));
//    }
//  }

//  /**
//   * Get User Model from Token (decode jwt token)
//   * @param {any} token
//   * @returns
//   */
//  private ConvertTokenToUser(token: any): Observable<User> {
//    try {
//      return observableOf(this.mapTokenToUserModel(this.jwtHelper.decodeToken(token)));
//    } catch (e) {
//      observableThrowError(e);
//    }
//  }

//  private mapTokenToUserModel(userToken: any) {
//    let user = <User>{
//      id: userToken.id,
//      birthday: userToken.birthdate,
//      lastName: userToken.family_name,
//      firstName: userToken.given_name,
//      photo: userToken.photo,
//      email: userToken.email,
//      gender: userToken.gender,
//      roles: userToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
//    };
//    return user;
//  }
//  /**
//   * Stores access token & refresh token.
//   *
//   * @param body The response of the request to the token endpoint
//   */
//  private store(body: AuthResponse): void {
//    // Stores access token in local storage to keep user signed in.
//    localStorage.setItem("access_token", body.accessToken);
//    // Stores refresh token in local storage.
//    localStorage.setItem("refreshToken", body.refreshToken);

//    // Decodes the token.
//    this.decodeToken();
//  }
//}
