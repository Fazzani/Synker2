import { Component } from '@angular/core';
import 'hammerjs';
import './app.component.css'
import { NotificationService } from '../../services/notification/notification.service';
import { MatSnackBar } from '@angular/material';

@Component({
    selector: 'app',
    templateUrl: './app.component.html'
})
export class AppComponent {
    color = 'primary';
    constructor(private notifService: NotificationService, public snackBar: MatSnackBar) {
        notifService.messages.subscribe(
            m => {
                console.log('new message ', m);
                this.snackBar.open(m.content, null, {
                    duration: 3000,
                })
            },
            error => console.warn(error));
    }
}
