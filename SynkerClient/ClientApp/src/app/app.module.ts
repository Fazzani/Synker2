import { JwtModule } from "@auth0/angular-jwt";
import { NgModule, APP_INITIALIZER } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { AppModuleMaterialModule } from "./app.module.material.module";

import { AppComponent } from "./components/app/app.component";
import { HomeComponent } from "./components/home/home.component";
import { MediaComponent, TvgMediaModifyDialog } from "./components/media/media.component";
import { EpgComponent, EpgModifyDialog } from "./components/epg/epg.component";
import { XmltvComponent, XmltvModifyDialog } from "./components/xmltv/xmltv.component";
import { DialogComponent } from "./components/shared/dialogs/dialog.component";

import { NavBarModule } from "./components/shared/navbar/navbar";
import { FlexLayoutModule } from "@angular/flex-layout";
import { DefaultHttpInterceptor } from "./infrastructure/DefaultHttpInterceptor";
import { PlaylistComponent } from "./components/playlist/playlist.component";
import { SearchPipe } from "./pipes/search.pipe";
import { SitePackComponent, SitePackModifyDialog } from "./components/sitepack/sitepack.component";
import { KeysPipe } from "./pipes/enumKey.pipe";
import { JwtInterceptor } from "./infrastructure/JwtInterceptor";
import { ToastrModule } from "ngx-toastr";
import { PushNotificationsModule } from "ng-push";
import { ClipboardModule } from "ngx-clipboard";
import { PlaylistTvgSitesDialog } from "./components/dialogs/playlistTvgSites/PlaylistTvgSitesDialog";
import { PlaylistBulkUpdate } from "./components/dialogs/playlistBulkUpdate/playlistBulkUpdate";
import { PlaylistAddDialog } from "./components/dialogs/playlistAddNew/playlist.add.component";
import { PlaylistDiffDialog } from "./components/dialogs/playlistDiff/playlist.diff.component";
import { PlaylistUpdateDialog } from "./components/dialogs/playlistUpdate/playlist.update.dialog";
import { GroupsDialog } from "./components/dialogs/group/groups.component";
import { MatchTvgDialog } from "./components/dialogs/matchTvg/matchTvg.component";
import { RegisterComponent, RegisterDialog } from "./components/dialogs/auth/RegisterDialog";
import { LoginDialog } from "./components/dialogs/auth/LoginDialog";
import { PlaylistInfosDialog } from "./components/dialogs/playlistInfos/playlist.infos.component";
import { UserComponent } from "./components/user/user.component";
import { AppRoutingModule } from "./app.module.routing";
import { LoaderComponent } from "./components/shared/loader/loader.component";
import { OverlayModule } from "@angular/cdk/overlay";
import { BrowserModule } from "@angular/platform-browser";
import { InitAppService } from "./services/initApp/InitAppService";
import { GroupComponent } from "./components/group/group.component";
import { PlaylistDetailResolver } from "./components/playlist/playlist.resolver";
import { UsersResolver } from "./components/admin/users/users.resolver";
import { HostsResolver } from "./components/admin/hosts/hosts.resolver";
import { HomeResolver } from "./components/home/home.resolver";
import { ServiceWorkerModule } from '@angular/service-worker';
import { environment } from '../environments/environment';
import { MediaWatchDialog } from "./components/dialogs/mediaWatch/media.watch.dialog";
import { AngularFireModule } from '@angular/fire';
import { AngularFireDatabaseModule } from '@angular/fire/database';
import { AngularFireAuthModule } from '@angular/fire/auth';
import { NotificationsComponent } from "./components/notifications/notifications.component";

export function tokenGetter() {
  return localStorage.getItem("access_token");
}

export function getAboutApplication(initService: InitAppService) {
  return () => initService.getAboutApplication();
}

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    MediaComponent,
    SitePackComponent,
    RegisterComponent,
    EpgComponent,
    XmltvComponent,
    UserComponent,
    NotificationsComponent,
    GroupComponent,
    LoaderComponent,
    TvgMediaModifyDialog,
    EpgModifyDialog,
    DialogComponent,
    LoginDialog,
    RegisterDialog,
    PlaylistComponent,
    PlaylistUpdateDialog,
    PlaylistBulkUpdate,
    PlaylistTvgSitesDialog,
    PlaylistAddDialog,
    PlaylistDiffDialog,
    SitePackModifyDialog,
    PlaylistInfosDialog,
    MediaWatchDialog,
    GroupsDialog,
    MatchTvgDialog,
    XmltvModifyDialog,
    SearchPipe,
    KeysPipe
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: "ng-cli-universal" }),
    BrowserAnimationsModule,
    PushNotificationsModule,
    AngularFireModule.initializeApp(environment.firebaseConfig, 'letslearn-dev'),
    AngularFireDatabaseModule,
    AngularFireAuthModule,
    FlexLayoutModule,
    ToastrModule.forRoot({
      positionClass: "toast-bottom-right",
      preventDuplicates: true,
      timeOut: 4000
    }),
    CommonModule,
    HttpClientModule,
    FormsModule,
    AppModuleMaterialModule,
    NavBarModule,
    ReactiveFormsModule,
    ClipboardModule,
    AppRoutingModule,
    OverlayModule,
    JwtModule.forRoot({
      config: {
        tokenGetter: tokenGetter,
        whitelistedDomains: ["localhost:56800", "api.synker.ovh"],
        blacklistedRoutes: [""]
      }
    }),
    ServiceWorkerModule.register('/ngsw-worker.js', { enabled: environment.production })
  ],
  entryComponents: [
    TvgMediaModifyDialog,
    EpgModifyDialog,
    LoginDialog,
    RegisterDialog,
    PlaylistBulkUpdate,
    PlaylistTvgSitesDialog,
    PlaylistAddDialog,
    PlaylistUpdateDialog,
    PlaylistDiffDialog,
    SitePackModifyDialog,
    GroupsDialog,
    MatchTvgDialog,
    PlaylistInfosDialog,
    XmltvModifyDialog,
    MediaWatchDialog
  ],
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
    {
      provide: APP_INITIALIZER,
      useFactory: getAboutApplication,
      multi: true,
      deps: [InitAppService]
    },
    PlaylistDetailResolver,
    HostsResolver,
    UsersResolver,
    HomeResolver
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
