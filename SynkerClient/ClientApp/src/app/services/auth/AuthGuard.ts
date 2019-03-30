import { Injectable } from '@angular/core';
import { CanActivate, Router, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from './auth.service';
import { tap } from 'rxjs/operators';

@Injectable()
export class AuthGuard implements CanActivate {

    constructor(
        private authService: AuthService) { }

  canActivate(route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot,) {
      return this.authService.canActivateProtectedRoutes$
        .pipe(tap(x => console.log('You tried to go to ' + state.url + ' and this guard said ' + x)));
    }
}
