import { Observable } from "rxjs";
import {finalize, tap} from 'rxjs/operators';
import { HttpRequest, HttpInterceptor, HttpEvent, HttpResponse, HttpErrorResponse, HttpHandler } from "@angular/common/http";
import { Router } from "@angular/router";
import { Injectable, Injector } from "@angular/core";
import { CommonService } from "../services/common/common.service";

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  private commonService: CommonService;

  constructor(private injector: Injector, private router: Router) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    this.commonService = this.injector.get(CommonService);

    this.commonService.displayLoader(true);

    return next
      .handle(request).pipe(
      tap(
        (event: HttpEvent<any>) => {
          console.log("IN JwtInterceptor", event);
          if (event instanceof HttpResponse) {
            // do stuff with response if you want
          }
        },
        (err: any) => {
          console.log("error in JwtInterceptor", err);
          if (err instanceof HttpErrorResponse) {
            let e = err as HttpErrorResponse;
            if (e.status == 401) this.commonService.displayError(err.statusText, "Unauthorized User");
            else this.commonService.displayError(err.statusText, typeof err.error === "string" ? err.error : err.error.Message);
          }
        }
      ),
      finalize(() => this.commonService.displayLoader(false)),);
  }
}
