import { Component, OnInit } from '@angular/core';
import { OAuthService } from "angular-oauth2-oidc";
import { Router } from "@angular/router";

@Component({
  selector: 'app-auth-callback',
  templateUrl: './auth-callback.component.html',
  styleUrls: ['./auth-callback.component.css']
})
export class AuthCallbackComponent implements OnInit {

  constructor(private oauthService: OAuthService, private router: Router) { }

  ngOnInit() {
    this.oauthService.loadDiscoveryDocumentAndTryLogin().then(_ => {
      if (!this.oauthService.hasValidIdToken() || !this.oauthService.hasValidAccessToken()) {
        this.oauthService.initImplicitFlow('login');
      } else {
        this.router.navigate(["/home"]);
      }
    });
  }
}
