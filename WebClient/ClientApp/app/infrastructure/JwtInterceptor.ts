import { HttpRequest, HttpInterceptor, HttpEvent, HttpResponse, HttpErrorResponse, HttpHandler } from '@angular/common/http';
import { AuthService } from '../services/auth/auth.service';
import { Observable } from 'rxjs/Observable';
import { Router } from '@angular/router';
import { Injectable, Injector } from '@angular/core';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    private authService: AuthService
    constructor(private injector: Injector, private router: Router) {
    }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        this.authService = this.injector.get(AuthService);

        return next.handle(request).do((event: HttpEvent<any>) => {
            console.log('IN JwtInterceptor', event);
            if (event instanceof HttpResponse) {
                // do stuff with response if you want
            }
        }, (err: any) => {
            console.log('error in JwtInterceptor', err);
            if (err instanceof HttpErrorResponse) {
                if (err.status === 401) {
                    console.log('JwtInterceptor 401');
                    this.authService.isAuthenticated().distinctUntilChanged().subscribe(isAuth => {
                        console.log(`JwtInterceptor 401 isAuth = ${isAuth}`);
                        if (!isAuth) {
                            this.authService.redirectUrl = this.router.routerState.snapshot.url;
                            this.router.navigate(['/signin', { dialog: 'signin', modal: 'true' }]);
                        }
                    });
                }
            }
        });
    }
}