import { ActivatedRouteSnapshot, Resolve } from "@angular/router";
import { Injectable } from "@angular/core";
import { Observable, from } from "rxjs";
import { QueryListBaseModel, PagedResult } from "../../types/common.type";
import { OAuthService } from 'angular-oauth2-oidc';
import { User } from "../../types/auth.type";
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class UserResolver implements Resolve<Observable<User>> {
  constructor(private oauthService: OAuthService) {}

  resolve(route: ActivatedRouteSnapshot) {
    return from(this.oauthService.loadUserProfile() as Promise<User>).pipe(map(User.FromUserProfile));
  }
}
