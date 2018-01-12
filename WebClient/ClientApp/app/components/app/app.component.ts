import { Component, OnInit, OnDestroy } from '@angular/core';
import 'hammerjs';
import './app.component.css'
import { NotificationService } from '../../services/notification/notification.service';
import { MatSnackBar } from '@angular/material';
import { CommonService } from '../../services/common/common.service';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';
import { Exception } from '../../types/common.type';
import { ToastyService } from 'ng2-toasty';
import * as variables from "../../variables";

@Component({
    selector: 'app',
    templateUrl: './app.component.html'
})
export class AppComponent implements OnInit, OnDestroy {

    color = 'primary';
    objLoaderStatus: boolean;

    constructor(private notifService: NotificationService, public snackBar: MatSnackBar, private commonService: CommonService,
        private authService: AuthService, private router: Router, private toastyService: ToastyService) {

        this.objLoaderStatus = false;

        notifService.messages.subscribe(
            m => {
                console.log('new message ', m);
                this.snackBar.open(m.content, null, {
                    duration: 3000,
                })
            },
            error => console.warn(error));
    }

    ngOnInit() {

        this.authService
            .isAuthenticated()
            .debounceTime(6000)
            .distinctUntilChanged()
            .subscribe(isAuth => {
                console.log(`----------- JwtInterceptor 401 isAuth = ${isAuth} current url ${this.router.routerState.snapshot.url} this.authService.redirectUrl: ${this.authService.redirectUrl}`);
                if (!isAuth && (this.router.routerState.snapshot.url != variables.SIGN_IN_URL && this.router.routerState.snapshot.url != variables.REGISTER_URL)) {
                    this.authService.redirectUrl = this.router.routerState.snapshot.url == variables.SIGN_IN_URL ? "/home" : this.router.routerState.snapshot.url;
                    this.router.navigate(['signin']);
                }
            });

        setTimeout(_ => {
            this.authService.authenticated.distinctUntilChanged().subscribe(isAuth => {
                if (!isAuth && this.router.url != variables.SIGN_IN_URL && this.router.url != variables.REGISTER_URL) {
                    this.authService.redirectUrl = this.router.routerState.snapshot.url;
                    this.router.navigate(['signin']);
                }
            });
        }, 1000);

        this.commonService.loaderStatus.distinctUntilChanged().debounceTime(2000).subscribe((val: boolean) => {
            console.log('new loader data : ', val);
            this.objLoaderStatus = val;
        });

        this.commonService.error.distinctUntilChanged().filter(err => err != null).subscribe((err: Exception) => {
            this.commonService.toastOptions.msg = err.message;
            this.commonService.toastOptions.title = err.title;
            this.toastyService.error(this.commonService.toastOptions);
        });
    }

    ngOnDestroy(): void {
    }
}
