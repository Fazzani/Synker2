
import { Component } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-should-login',
  templateUrl: "./should-login.component.html"
})
export class ShouldLoginComponent {
  constructor(private authService: OAuthService) { }

  public login($event) {
    $event.preventDefault();
    this.authService.initImplicitFlow();
  }
}
