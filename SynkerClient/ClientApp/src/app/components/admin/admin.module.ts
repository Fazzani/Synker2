import { Routes, RouterModule } from "@angular/router";
import { AdminComponent } from "./admin.component";
import { UsersComponent } from "./users/users.component";
import { UsersResolver } from "./users/users.resolver";
import { HostsComponent } from "./hosts/hosts.component";
import { HostsResolver } from "./hosts/hosts.resolver";
import { AdminDashboardComponent } from "./dashboard/admin.dashboard.component";
import { NgModule } from "@angular/core";
import { AppModuleMaterialModule } from "../../app.module.material.module";
import { FlexLayoutModule } from "@angular/flex-layout";
import { CommonModule } from "@angular/common";
import { AuthGuard } from '../../services/auth/AuthGuard';
import { OAuthModule } from 'angular-oauth2-oidc';
import { authModuleConfig } from '../../../environments/environment';

const adminRoutes: Routes = [
  {
    path: "",
    component: AdminComponent,
    canActivate: [AuthGuard],
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
    OAuthModule.forRoot(authModuleConfig),
    AppModuleMaterialModule,
    CommonModule,
    FlexLayoutModule
  ],
  exports: [RouterModule],
  providers: [
    HostsResolver,
    UsersResolver
  ]
})
export class AdminModule {}
