import { HttpRequest, HttpInterceptor, HttpEvent, HttpResponse, HttpErrorResponse, HttpHandler } from '@angular/common/http';
import { AuthService } from '../services/auth/auth.service';
import { Observable } from 'rxjs/Observable';
import { Router } from '@angular/router';
import { Injectable, Injector } from '@angular/core';
import { CommonService } from '../services/common/common.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    private authService: AuthService
    private commonService: CommonService;

    constructor(private injector: Injector, private router: Router) {
    }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        this.authService = this.injector.get(AuthService);
        this.commonService = this.injector.get(CommonService);

        this.commonService.displayLoader(true)

        return next.handle(request).do((event: HttpEvent<any>) => {
            console.log('IN JwtInterceptor', event);
            if (event instanceof HttpResponse) {
                // do stuff with response if you want
            }
        }, (err: any) => {
            console.log('error in JwtInterceptor', err);
            if (err instanceof HttpErrorResponse) {
                let e = err as HttpErrorResponse;
                if (e.status == 401)
                    this.commonService.displayError(err.statusText, "User unauthorized");
                else
                    this.commonService.displayError(err.statusText, typeof err.error === "string" ? err.error : err.error.Message);
            }
        }).finally(() => this.commonService.displayLoader(false));
    }
}