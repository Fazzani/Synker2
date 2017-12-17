import { Component, OnInit } from '@angular/core';
import 'hammerjs';
import './app.component.css'
import { NotificationService } from '../../services/notification/notification.service';
import { MatSnackBar } from '@angular/material';
import { CommonService } from '../../services/common/common.service';

@Component({
    selector: 'app',
    templateUrl: './app.component.html',
    providers: [CommonService]
})
export class AppComponent implements OnInit {
    color = 'primary';
    objLoaderStatus: boolean;

    constructor(private notifService: NotificationService, public snackBar: MatSnackBar, private commonService: CommonService) {

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
        this.commonService.loaderStatus.subscribe((val: boolean) => {
            console.log('new loader data : ', val);
            this.objLoaderStatus = val;
        });
    }
}
