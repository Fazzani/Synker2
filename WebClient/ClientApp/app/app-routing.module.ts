﻿import { NgModule } from "@angular/core";
import { HomeComponent } from "./components/home/home.component";
import { LoginRouteGuard } from "./services/auth/loginRouteGuard.service";
import { Routes, RouterModule } from "@angular/router";
import { EpgComponent } from "./components/epg/epg.component";
import { XmltvComponent } from "./components/xmltv/xmltv.component";
import { SitePackComponent } from "./components/sitepack/sitepack.component";
import { PlaylistComponent } from "./components/playlist/playlist.component";
import { MessagesComponent } from "./components/messages/messages.component";
import { UsersComponent } from "./components/admin/users/users.component";
import { DialogComponent } from "./components/shared/dialogs/dialog.component";
import { RegisterComponent } from "./components/dialogs/auth/RegisterDialog";
import { MediaComponent } from "./components/media/media.component";
import { AdminComponent } from "./components/admin/admin.component";
import { AdminDashboardComponent } from "./components/admin/dashboard/admin.dashboard.component";


const appRoutes: Routes = [
    { path: 'home', component: HomeComponent, canActivate: [LoginRouteGuard] },
    { path: 'tvgmedia', component: MediaComponent, canActivate: [LoginRouteGuard] },
    { path: 'epg', component: EpgComponent, canActivate: [LoginRouteGuard] },
    { path: 'xmltv', component: XmltvComponent, canActivate: [LoginRouteGuard] },
    { path: 'sitepack', component: SitePackComponent, canActivate: [LoginRouteGuard] },
    { path: 'playlist/:id', component: PlaylistComponent, canActivate: [LoginRouteGuard] },
    { path: 'messages', component: MessagesComponent, canActivate: [LoginRouteGuard] },
    { path: 'signin', component: DialogComponent },
    { path: 'register', component: RegisterComponent },
    {
        path: 'admin', component: AdminComponent, canActivate: [LoginRouteGuard],
        children: [
            {
                path: '',
                children: [
                    {
                        path: 'users',
                        component: UsersComponent
                    }, {
                        path: '',
                        component: AdminDashboardComponent 
                    }
                ]
            }
        ]
    },
    { path: '', redirectTo: '/home', pathMatch: 'full' },
    { path: '**', redirectTo: '/home' }
];

@NgModule({
    imports: [
        RouterModule.forRoot(
            appRoutes,
            { enableTracing: false } // <-- debugging purposes only
        )
    ],
    exports: [
        RouterModule
    ]
})
export class AppRoutingModule  {
}