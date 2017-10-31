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
import { DialogComponent, LoginDialog, RegisterDialog } from './components/shared/dialogs/dialog.component';
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

const appRoutes: Routes = [
    { path: '', redirectTo: 'home', pathMatch: 'full' },
    { path: 'home', component: HomeComponent },
    { path: 'tvgmedia', component: MediaComponent, canActivate: [LoginRouteGuard] },
    { path: 'epg', component: EpgComponent, canActivate: [LoginRouteGuard] },
    { path: 'xmltv', component: XmltvComponent, canActivate: [LoginRouteGuard] },
    { path: 'signin', component: DialogComponent },
    { path: 'register', component: DialogComponent },
    { path: '**', redirectTo: 'home' }
];

@NgModule({
    declarations: [
        AppComponent,
        HomeComponent,
        MediaComponent,
        EpgComponent,
        XmltvComponent,
        TvgMediaModifyDialog,
        EpgModifyDialog,
        DialogComponent,
        LoginDialog,
        RegisterDialog
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
        RouterModule.forRoot(appRoutes, { enableTracing: false })
    ],
    entryComponents: [TvgMediaModifyDialog, EpgModifyDialog, LoginDialog, RegisterDialog],
    providers: [
        CommonService,
        TvgMediaService,
        CommonService,
        MessageService,
        NotificationService,
        XmltvService,
        AuthService,
        LoginRouteGuard,
        EpgService,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: DefaultHttpInterceptor,
            multi: true
        }, {
            provide: HTTP_INTERCEPTORS,
            useClass: TokenInterceptor,
            multi: true
        }]
})
export class AppModuleShared {
    constructor(private _appRef: ApplicationRef) { }
}
