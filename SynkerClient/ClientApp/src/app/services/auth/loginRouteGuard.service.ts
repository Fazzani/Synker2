import { Observable } from "rxjs";
import { map } from "rxjs/operators";
import { Router, CanActivate, CanActivateChild, ActivatedRouteSnapshot, RouterStateSnapshot } from "@angular/router";
import { Injectable } from "@angular/core";
import { AuthService } from "./auth.service";

@Injectable()
export class LoginRouteGuard implements CanActivate, CanActivateChild {
  constructor(private authService: AuthService, private router: Router) {}

  canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | Observable<boolean> | Promise<boolean> {
    return this.authService.isAuthenticated().pipe(
      map(x => {
        if (!x) {
          // Stores the attempted URL for redirecting.
          let url: string = state.url;
          this.authService.redirectUrl = url;
          // Not signed in so redirects to signin page.
          this.router.navigate(["/signin"]);
          return false;
        }
        return x;
      })
    );
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | Observable<boolean> | Promise<boolean> {
    return this.authService.isAuthenticated().pipe(
      map(x => {
        if (!x) {
          // Stores the attempted URL for redirecting.
          console.log(`Stores the attempted URL for redirecting`);
          let url: string = state.url;
          this.authService.redirectUrl = url;
          // Not signed in so redirects to signin page.
          this.router.navigate(["/signin"]);
          return false;
        }
        return x;
      })
    );
  }
}
