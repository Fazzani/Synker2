import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { OAuthService } from "angular-oauth2-oidc";

@Injectable()
export class AuthGuard implements CanActivate {

    constructor(
        private router: Router,
        private oauthService: OAuthService) { }

    canActivate() {
      if (this.oauthService.hasValidIdToken() || this.oauthService.hasValidAccessToken()) {
        console.log('User has valid token');
            return true;
        }

        this.router.navigate(["/unauthorized"]);
    }
}
