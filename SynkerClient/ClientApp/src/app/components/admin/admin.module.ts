import { Routes, RouterModule } from "@angular/router";
import { AdminComponent } from "./admin.component";
//import { LoginRouteGuard } from "../../services/auth/loginRouteGuard.service";
//import { AuthorizedRouteGuard } from "../../services/auth/authorizedRouteGuard.service";
import { UsersComponent } from "./users/users.component";
import { UsersResolver } from "./users/users.resolver";
import { HostsComponent } from "./hosts/hosts.component";
import { HostsResolver } from "./hosts/hosts.resolver";
import { AdminDashboardComponent } from "./dashboard/admin.dashboard.component";
import { NgModule } from "@angular/core";
import { AppModuleMaterialModule } from "../../app.module.material.module";
import {  HTTP_INTERCEPTORS } from "@angular/common/http";
import { FlexLayoutModule } from "@angular/flex-layout";
import { CommonModule } from "@angular/common";
import { JwtInterceptor } from "@auth0/angular-jwt";
import { DefaultHttpInterceptor } from "../../infrastructure/DefaultHttpInterceptor";
import { AuthGuardService } from '../../services/auth/auth-guard.service';

const adminRoutes: Routes = [
  {
    path: "",
    component: AdminComponent,
    canActivate: [AuthGuardService],
    children: [
      {
        path: "",
        children: [
          {
            path: "users",
            component: UsersComponent,
            resolve: { data: UsersResolver }
          },
          {
            path: "hosts",
            component: HostsComponent,
            resolve: { data: HostsResolver }
          },
          {
            path: "",
            component: AdminDashboardComponent
          }
        ]
      }
    ]
  }
];

@NgModule({
  declarations: [AdminDashboardComponent, AdminComponent, HostsComponent, UsersComponent],
  imports: [
    RouterModule.forChild(adminRoutes),
    AppModuleMaterialModule,
    CommonModule,
    FlexLayoutModule
  ],
  exports: [RouterModule],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: DefaultHttpInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true
    },
    HostsResolver,
    UsersResolver
  ]
})
export class AdminModule {}
