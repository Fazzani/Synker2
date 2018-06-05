//import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
//import { HomeComponent } from './home/home.component';
//import { BrowserAnimationsModule } from '@angular/platform-browser/animations';



import { NgModule } from '@angular/core';
//import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule, Routes } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppModuleMaterialModule } from './app.module.material.module';

import { AppComponent } from './components/app/app.component';
import { HomeComponent } from './components/home/home.component';
import { MediaComponent, TvgMediaModifyDialog } from './components/media/media.component';
import { EpgComponent, EpgModifyDialog } from './components/epg/epg.component';
import { XmltvComponent, XmltvModifyDialog } from './components/xmltv/xmltv.component';
import { DialogComponent } from './components/shared/dialogs/dialog.component';
import { LoginRouteGuard } from './services/auth/loginRouteGuard.service';

import { AuthService } from './services/auth/auth.service';
import { NotificationService } from './services/notification/notification.service';
import { TvgMediaService } from './services/tvgmedia/tvgmedia.service';
import { EpgService } from './services/epg/epg.service';
import { XmltvService } from './services/xmltv/xmltv.service';
import { MessageService } from './services/message/message.service';
import { CommonService } from './services/common/common.service';
import { BaseService } from './services/base/base.service';
import { NavBarModule } from './components/shared/navbar/navbar';

import { TokenInterceptor } from './services/auth/token.interceptor';
import { DefaultHttpInterceptor } from './infrastructure/DefaultHttpInterceptor'
import { MediaRefService } from './services/mediaref/mediaref.service';
import { PlaylistService } from './services/playlists/playlist.service';
import { PlaylistComponent } from "./components/playlist/playlist.component";
import { PiconService } from './services/picons/picons.service';
import { SearchPipe } from "./pipes/search.pipe";
import { SitePackService } from './services/sitepack/sitepack.service';
import { SitePackComponent, SitePackModifyDialog } from './components/sitepack/sitepack.component';
import { KeysPipe } from './pipes/enumKey.pipe';
import { JwtInterceptor } from './infrastructure/JwtInterceptor';
import { ToastrModule  } from 'ngx-toastr'
import { PushNotificationsModule } from 'ng-push';
import { ClipboardModule } from 'ngx-clipboard';
//import { BASE_URL, BASE_API_URL, BASE_WS_URL } from './variables';
//import { NgHttpLoaderModule } from 'ng-http-loader/ng-http-loader.module';
import { PlaylistTvgSitesDialog } from './components/dialogs/playlistTvgSites/PlaylistTvgSitesDialog';
import { PlaylistBulkUpdate } from './components/dialogs/playlistBulkUpdate/playlistBulkUpdate';
import { PlaylistAddDialog } from './components/dialogs/playlistAddNew/playlist.add.component';
import { PlaylistDiffDialog } from './components/dialogs/playlistDiff/playlist.diff.component';
import { PlaylistUpdateDialog } from './components/dialogs/playlistUpdate/playlist.update.dialog';
import { GroupsDialog } from './components/dialogs/group/groups.component';
import { MatchTvgDialog } from './components/dialogs/matchTvg/matchTvg.component';
import { RegisterComponent, RegisterDialog } from './components/dialogs/auth/RegisterDialog';
import { LoginDialog } from './components/dialogs/auth/LoginDialog';
import { XtreamService } from './services/xtream/xtream.service';
import { PlaylistInfosDialog } from './components/dialogs/playlistInfos/playlist.infos.component';
import { MessagesComponent } from './components/messages/messages.component';
import { UsersComponent } from './components/admin/users/users.component';
import { UsersService } from './services/admin/users.service';
import { AdminComponent } from './components/admin/admin.component';
import { AdminDashboardComponent } from './components/admin/dashboard/admin.dashboard.component';
import { UserComponent } from './components/user/user.component';
import { AppRoutingModule } from './app.module.routing';
import { LoaderComponent } from './components/shared/loader/loader.component';
import { HostsService } from './services/admin/hosts.service';
import { HostsComponent } from './components/admin/hosts/hosts.component';

@NgModule({
  declarations: [
   // BrowserModule,
    AppComponent,
    HomeComponent,
    MediaComponent,
    SitePackComponent,
    RegisterComponent,
    EpgComponent,
    XmltvComponent,
    MessagesComponent,
    AdminComponent,
    UserComponent,
    AdminDashboardComponent,
    UsersComponent,
    HostsComponent,
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
    GroupsDialog,
    MatchTvgDialog,
    SearchPipe,
    KeysPipe
  ],
  imports: [
    //BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    BrowserAnimationsModule,
    PushNotificationsModule,
    ToastrModule.forRoot({
      positionClass: 'toast-bottom-right',
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
    AppRoutingModule
  ],
  entryComponents: [TvgMediaModifyDialog, EpgModifyDialog, LoginDialog, RegisterDialog, PlaylistBulkUpdate,
    PlaylistTvgSitesDialog, PlaylistAddDialog, PlaylistUpdateDialog, PlaylistDiffDialog, SitePackModifyDialog, GroupsDialog,
    MatchTvgDialog, PlaylistInfosDialog],
  providers: [
    CommonService,
    TvgMediaService,
    MessageService,
    NotificationService,
    XmltvService,
    AuthService,
    LoginRouteGuard,
    MediaRefService,
    PlaylistService,
    PiconService,
    UsersService,
    XtreamService,
    SitePackService,
    HostsService,
    EpgService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: DefaultHttpInterceptor,
      multi: true
    }, {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true
    }, {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true
    }],
bootstrap: [AppComponent]
})
export class AppModule { }