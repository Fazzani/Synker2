import { Injectable, Inject, Optional } from '@angular/core';
import { OAuthService, OAuthStorage, OAuthModuleConfig, OAuthResourceServerErrorHandler } from 'angular-oauth2-oidc';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable()
export class DefaultOAuthInterceptor implements HttpInterceptor {

  constructor(
    private authStorage: OAuthStorage,
    private errorHandler: OAuthResourceServerErrorHandler,
    @Optional() private moduleConfig: OAuthModuleConfig
  ) {
  }

  private checkUrl(url: string): boolean {
    let found = this.moduleConfig.resourceServer.allowedUrls.find((u: string) => url.startsWith(u));
    return !!found;
  }

  public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    let url = req.url.toLowerCase();
    if (!this.moduleConfig) return next.handle(req);
    if (!this.moduleConfig.resourceServer) return next.handle(req);
    if (!this.moduleConfig.resourceServer.allowedUrls) return next.handle(req);
    if (!this.checkUrl(url)) return next.handle(req);

    let sendAccessToken = this.moduleConfig.resourceServer.sendAccessToken;

    if (sendAccessToken) {

      let token = this.authStorage.getItem('access_token');
      let header = 'Bearer ' + token;

      let headers = req.headers
        .set('Authorization', header);

      req = req.clone({ headers });
    }

    return next.handle(req).pipe(tap(
      (event: HttpEvent<any>) => {
        if (event instanceof HttpResponse) {
          console.log('receiving response...');
          console.log(event);
        }
      },
      (error: any) => {
        if (error instanceof HttpErrorResponse) {
          console.log('receiving error response...');
          console.error(error);
        }
      }
    ));

  }
}
