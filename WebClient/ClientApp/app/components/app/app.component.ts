import { Component, OnInit, OnDestroy } from '@angular/core';
import 'hammerjs';
import './app.component.css'
import { NotificationService } from '../../services/notification/notification.service';
import { MatSnackBar } from '@angular/material';
import { CommonService } from '../../services/common/common.service';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';
import { Exception } from '../../types/common.type';

@Component({
    selector: 'app',
    templateUrl: './app.component.html'
})
export class AppComponent implements OnInit, OnDestroy {

    color = 'primary';
    objLoaderStatus: boolean;

    constructor(private notifService: NotificationService, public snackBar: MatSnackBar, private commonService: CommonService,
        private authService: AuthService, private router: Router) {

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
        this.authService.authenticated.distinctUntilChanged().subscribe(isAuth => {
            if (!isAuth) {
                this.authService.redirectUrl = this.router.routerState.snapshot.url;
                this.router.navigate(['/signin', { dialog: 'signin', modal: 'true' }]);
            }
        });

        this.commonService.loaderStatus.distinctUntilChanged().debounceTime(2000).subscribe((val: boolean) => {
            console.log('new loader data : ', val);
            this.objLoaderStatus = val;
        });

        this.commonService.error.distinctUntilChanged().filter(err => err != null).subscribe((err: Exception) => {
            this.snackBar.open(err.message);
        });
    }

    ngOnDestroy(): void {
    }
}
