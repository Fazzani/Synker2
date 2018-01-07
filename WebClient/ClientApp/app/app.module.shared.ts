import { ApplicationRef, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
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
import { DialogComponent, LoginDialog, RegisterDialog, RegisterComponent } from './components/shared/dialogs/dialog.component';
import { LoginRouteGuard } from './services/auth/loginRouteGuard.service';

import { AuthService } from './services/auth/auth.service';
import { NotificationService } from './services/notification/notification.service';
import { TvgMediaService } from './services/tvgmedia/tvgmedia.service';
import { EpgService } from './services/epg/epg.service';
import { XmltvService } from './services/xmltv/xmltv.service';
import { MessageService } from './services/message/message.service';
import { CommonService } from './services/common/common.service';
import { BaseService } from './services/base/base.service';
import { NgHttpLoaderModule } from 'ng-http-loader/ng-http-loader.module';
import { NavBarModule } from './components/shared/navbar/navbar';

import { BASE_URL, BASE_API_URL, BASE_WS_URL } from './variables';
import { TokenInterceptor } from './services/auth/token.interceptor';
import { DefaultHttpInterceptor } from './infrastructure/DefaultHttpInterceptor'
import { MediaRefService } from './services/mediaref/mediaref.service';
import { MediaRefModifyDialog, MediaRefComponent } from './components/mediaref/mediaref.component';
import { PlaylistService } from './services/playlists/playlist.service';
import { ClipboardModule } from 'ngx-clipboard';
import { PlaylistComponent, PlaylistModifyDialog, TvgMediaListModifyDialog, TvgSitesListModifyDialog } from "./components/playlist/playlist.component";
import { PiconService } from './services/picons/picons.service';
import { SearchPipe } from "./pipes/search.pipe";
import { PlaylistAddDialog } from './components/playlist/playlist.add.component';
import { PlaylistDiffDialog } from './components/playlist/playlist.diff.component';
import { SitePackService } from './services/sitepack/sitepack.service';
import { SitePackComponent, SitePackModifyDialog } from './components/sitepack/sitepack.component';
import { KeysPipe } from './pipes/enumKey.pipe';
import { JwtInterceptor } from './infrastructure/JwtInterceptor';
import { GroupsDialog } from './components/group/groups.component';
import { MatchTvgDialog } from './components/matchTvg/matchTvg.component';

const appRoutes: Routes = [
    { path: 'home', component: HomeComponent, canActivate: [LoginRouteGuard] },
    { path: 'tvgmedia', component: MediaComponent, canActivate: [LoginRouteGuard] },
    { path: 'epg', component: EpgComponent, canActivate: [LoginRouteGuard] },
    { path: 'xmltv', component: XmltvComponent, canActivate: [LoginRouteGuard] },
    { path: 'sitepack', component: SitePackComponent, canActivate: [LoginRouteGuard] },
    { path: 'playlist/:id', component: PlaylistComponent, canActivate: [LoginRouteGuard] },
    { path: 'signin', component: DialogComponent },
    { path: 'register', component: RegisterComponent },
    { path: '', redirectTo: '/home', pathMatch: 'full' },
    { path: '**', redirectTo: '/home' }
];

@NgModule({
    declarations: [
        AppComponent,
        HomeComponent,
        MediaComponent,
        MediaRefComponent,
        SitePackComponent,
        RegisterComponent,
        EpgComponent,
        XmltvComponent,
        TvgMediaModifyDialog,
        EpgModifyDialog,
        DialogComponent,
        LoginDialog,
        RegisterDialog,
        MediaRefModifyDialog,
        PlaylistComponent,
        PlaylistModifyDialog,
        TvgMediaListModifyDialog,
        TvgSitesListModifyDialog,
        PlaylistAddDialog,
        PlaylistDiffDialog,
        SitePackModifyDialog,
        GroupsDialog,
        MatchTvgDialog,
        SearchPipe,
        KeysPipe
    ],
    imports: [
        BrowserModule,
        // BrowserAnimationsModule,
        CommonModule,
        HttpClientModule,
        FormsModule,
        AppModuleMaterialModule,
        NavBarModule,
        ReactiveFormsModule,
        ClipboardModule,
        RouterModule.forRoot(appRoutes, { enableTracing: true })
    ],
    entryComponents: [TvgMediaModifyDialog, EpgModifyDialog, LoginDialog, RegisterDialog, MediaRefModifyDialog, TvgMediaListModifyDialog,
        TvgSitesListModifyDialog, PlaylistAddDialog, PlaylistModifyDialog, PlaylistDiffDialog, SitePackModifyDialog, GroupsDialog, MatchTvgDialog],
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
        SitePackService,
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
        }]
})
export class AppModuleShared {
    constructor(private _appRef: ApplicationRef) { }
}
