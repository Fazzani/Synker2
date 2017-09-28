import { CanActivate, CanActivateChild } from '@angular/router';
import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';

@Injectable()
export class LoginRouteGuard implements CanActivate, CanActivateChild {

    constructor(private loginService: AuthService) {}

  canActivate() {
    return this.loginService.isLoggedIn();
  }
}