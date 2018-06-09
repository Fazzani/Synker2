// import { Observable } from "rxjs";
// import { Injectable, Injector } from "@angular/core";
// import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from "@angular/common/http";
// import { AuthService } from "./auth.service";

// @Injectable()
// export class TokenInterceptor implements HttpInterceptor {
//   auth: AuthService;
//   constructor(private injector: Injector) {}

//   intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
//     this.auth = this.injector.get(AuthService);

//     console.log(TokenInterceptor.name);

//     request = request.clone({
//       setHeaders: {
//         Authorization: `Bearer ${this.auth.getToken()}`
//       }
//     });
//     return next.handle(request);
//   }
// }
