import { Router, CanActivate, CanActivateChild, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/take';
import 'rxjs/add/operator/map';

@Injectable()
export class LoginRouteGuard implements CanActivate, CanActivateChild {


    constructor(private authService: AuthService, private router: Router) { }

    canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | Observable<boolean> | Promise<boolean> {
        return this.authService.authenticated.map(x => {
            if (!x) {
                // Stores the attempted URL for redirecting.  
                let url: string = state.url;
                this.authService.redirectUrl = url;
                // Not signed in so redirects to signin page.  
                this.router.navigate(['/signin', { dialog: 'signin', modal: 'true' }]);
                return false;
            }
            return x;
        });
    }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | Observable<boolean> | Promise<boolean> {
        return this.authService.authenticated.map(x => {
            if (!x) {
                // Stores the attempted URL for redirecting.
                console.log(`Stores the attempted URL for redirecting`);
                let url: string = state.url;
                this.authService.redirectUrl = url;
                // Not signed in so redirects to signin page.  
                this.router.navigate(['/signin', { dialog: 'signin', modal: 'true' }]);
                return false;
            }
            return x;
        });
    }
}