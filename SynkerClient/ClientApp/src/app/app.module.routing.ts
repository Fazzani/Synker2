﻿import { NgModule } from "@angular/core";
import { HomeComponent } from "./components/home/home.component";
import { LoginRouteGuard } from "./services/auth/loginRouteGuard.service";
import { Routes, RouterModule } from "@angular/router";
import { EpgComponent } from "./components/epg/epg.component";
import { XmltvComponent } from "./components/xmltv/xmltv.component";
import { SitePackComponent } from "./components/sitepack/sitepack.component";
import { PlaylistComponent } from "./components/playlist/playlist.component";
import { MessagesComponent } from "./components/messages/messages.component";
import { DialogComponent } from "./components/shared/dialogs/dialog.component";
import { RegisterComponent } from "./components/dialogs/auth/RegisterDialog";
import { MediaComponent } from "./components/media/media.component";
import { UserComponent } from "./components/user/user.component";
import { GroupComponent } from "./components/group/group.component";
import { PlaylistDetailResolver } from "./components/playlist/playlist.resolver";
import { MessagesResolver } from "./components/messages/messages.resolver";
import { HomeResolver } from "./components/home/home.resolver";

const appRoutes: Routes = [
  { path: "", component: HomeComponent, canActivate: [LoginRouteGuard], resolve: { data: HomeResolver } },
  { path: "tvgmedia", component: MediaComponent, canActivate: [LoginRouteGuard] },
  { path: "admin", loadChildren: "./components/admin/admin.module#AdminModule" },
  { path: "epg", component: EpgComponent, canActivate: [LoginRouteGuard] },
  { path: "xmltv", component: XmltvComponent, canActivate: [LoginRouteGuard] },
  { path: "sitepack", component: SitePackComponent, canActivate: [LoginRouteGuard] },
  { path: "playlist/:id", component: PlaylistComponent, canActivate: [LoginRouteGuard], resolve: { data: PlaylistDetailResolver } },
  { path: "playlist/:id/groups", component: GroupComponent, canActivate: [LoginRouteGuard] },
  { path: "messages", component: MessagesComponent, canActivate: [LoginRouteGuard], resolve: { data: MessagesResolver } },
  { path: "me", component: UserComponent, canActivate: [LoginRouteGuard] },
  { path: "signin", component: DialogComponent },
  { path: "register", component: RegisterComponent },
  //{ path: "", redirectTo: "", pathMatch: "full" },
  { path: "**", redirectTo: "", pathMatch: "full" }
];

@NgModule({
  imports: [
    RouterModule.forRoot(
      appRoutes,
      { enableTracing: false } // <-- debugging purposes only
    )
  ],
  exports: [RouterModule]
})
export class AppRoutingModule {}
