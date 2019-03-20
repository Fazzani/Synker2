import { NgModule } from "@angular/core";
import { HomeComponent } from "./components/home/home.component";
import { Routes, RouterModule } from "@angular/router";
import { EpgComponent } from "./components/epg/epg.component";
import { XmltvComponent } from "./components/xmltv/xmltv.component";
import { SitePackComponent } from "./components/sitepack/sitepack.component";
import { PlaylistComponent } from "./components/playlist/playlist.component";
import { DialogComponent } from "./components/shared/dialogs/dialog.component";
import { RegisterComponent } from "./components/dialogs/auth/RegisterDialog";
import { MediaComponent } from "./components/media/media.component";
import { UserComponent } from "./components/user/user.component";
import { GroupComponent } from "./components/group/group.component";
import { PlaylistDetailResolver } from "./components/playlist/playlist.resolver";
import { HomeResolver } from "./components/home/home.resolver";
import { NotificationsComponent } from "./components/notifications/notifications.component";
import { AuthCallbackComponent } from './components/auth-callback/auth-callback.component';
import { AuthGuard } from './services/auth/AuthGuard';
import { UnauthorizedComponent } from './components/unauthorized/unauthorized.component';

const appRoutes: Routes = [
  { path: "home", component: HomeComponent, canActivate: [AuthGuard], resolve: { data: HomeResolver } },
  { path: "tvgmedia", component: MediaComponent, canActivate: [AuthGuard] },
  { path: "admin", loadChildren: "./components/admin/admin.module#AdminModule" },
  { path: "epg", component: EpgComponent, canActivate: [AuthGuard] },
  { path: "xmltv", component: XmltvComponent },
  { path: "unauthorized", component: UnauthorizedComponent  },
  { path: "sitepack", component: SitePackComponent, canActivate: [AuthGuard] },
  { path: "playlist/:id", component: PlaylistComponent, canActivate: [AuthGuard], resolve: { data: PlaylistDetailResolver } },
  { path: "playlist/:id/groups", component: GroupComponent, canActivate: [AuthGuard] },
  { path: "notifications", component: NotificationsComponent, canActivate: [AuthGuard] },
  { path: "me", component: UserComponent, canActivate: [AuthGuard] }, 
  { path: "signin", component: DialogComponent },
  { path: "register", component: RegisterComponent },
  { path: 'auth-callback', component: AuthCallbackComponent },
  { path: "", redirectTo: "/home", pathMatch: "full" },
  { path: "**", redirectTo: "/home", pathMatch: "full" }
];

@NgModule({
  imports: [
    RouterModule.forRoot(
      appRoutes,
      { enableTracing: false } // <-- debugging purposes only
    )
  ],
  providers: [AuthGuard], // -- AQUI
  exports: [RouterModule]
})
export class AppRoutingModule {}
