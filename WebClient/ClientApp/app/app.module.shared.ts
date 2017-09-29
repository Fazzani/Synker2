import { ApplicationRef, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule, Routes } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppModuleMaterialModule } from './app.module.material.module';

import { AppComponent } from './components/app/app.component';
import { HomeComponent } from './components/home/home.component';
import { MediaComponent, TvgMediaModifyDialog } from './components/media/media.component';
import { EpgComponent, EpgModifyDialog } from './components/epg/epg.component';

import { AuthService } from './services/auth/auth.service';
import { TvgMediaService } from './services/tvgmedia/tvgmedia.service';
import { EpgService } from './services/epg/epg.service';
import { MessageService } from './services/message/message.service';
import { CommonService } from './services/common/common.service';
import { BaseService } from './services/base/base.service';
import { NgHttpLoaderModule } from 'ng-http-loader/ng-http-loader.module';
import { NavBarModule } from './components/shared/navbar/navbar';
import { DefaultHttpInterceptor } from './infrastructure/DefaultHttpInterceptor'

const appRoutes: Routes = [
    { path: '', redirectTo: 'home', pathMatch: 'full' },
    { path: 'home', component: HomeComponent },
    { path: 'tvgmedia', component: MediaComponent },
    { path: 'epg', component: EpgComponent },
    { path: '**', redirectTo: 'home' }
];

@NgModule({
    declarations: [
        AppComponent,
        HomeComponent,
        MediaComponent,
        EpgComponent,
        TvgMediaModifyDialog,
        EpgModifyDialog
    ],
    imports: [
        BrowserModule,
        // BrowserAnimationsModule,
        CommonModule,
        HttpClientModule,
        FormsModule,
        AppModuleMaterialModule,
        NavBarModule,
        RouterModule.forRoot(appRoutes, { enableTracing: false })
    ],
    entryComponents: [TvgMediaModifyDialog, EpgModifyDialog],
    providers: [
        CommonService,
        TvgMediaService,
        CommonService,
        MessageService,
        AuthService,
        EpgService,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: DefaultHttpInterceptor,
            multi: true
        }]
})
export class AppModuleShared {
    constructor(private _appRef: ApplicationRef) { }
}
