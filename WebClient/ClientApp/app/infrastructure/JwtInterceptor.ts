import { HttpRequest, HttpInterceptor, HttpEvent, HttpResponse, HttpErrorResponse, HttpHandler } from '@angular/common/http';
import { AuthService } from '../services/auth/auth.service';
import { Observable } from 'rxjs/Observable';
import { Router } from '@angular/router';
import { Injectable, Injector } from '@angular/core';
import { CommonService } from '../services/common/common.service';
import { ToastyService, ToastOptions } from 'ng2-toasty';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    toastyService: ToastyService;
    private authService: AuthService
    private commonService: CommonService;

    constructor(private injector: Injector, private router: Router) {
    }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        this.authService = this.injector.get(AuthService);
        this.commonService = this.injector.get(CommonService);
        this.toastyService = this.injector.get(ToastyService);
        this.commonService.displayLoader(true)

        var toastOptions: ToastOptions = {
            title: "My title",
            msg: "The message",
            showClose: true,
            timeout: 4000,
            theme: 'default'
        };

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
                        if (!isAuth && (this.router.routerState.snapshot.url != 'signin' && this.router.routerState.snapshot.url != 'register')) {
                            this.authService.redirectUrl = this.router.routerState.snapshot.url;
                            this.router.navigate(['signin']);
                        }
                    });
                } else {
                    toastOptions.title = err.statusText;
                    toastOptions.msg = typeof err.error === "string" ? err.error : err.error.Message;
                }

                this.toastyService.error(toastOptions);
            }
            }).finally(()=> this.commonService.displayLoader(false));
    }
}