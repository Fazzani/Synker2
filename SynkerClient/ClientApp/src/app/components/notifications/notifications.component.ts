import { Component, OnInit, OnDestroy } from "@angular/core";
import { MatSnackBar } from "@angular/material";
import { NotificationService } from "../../services/notification/notification.service";
import FirebaseNotification from "../../types/firebase.type";
import { User } from "../../types/auth.type";
import { map } from "rxjs/operators";
import { Observable } from "rxjs";
import * as moment from "moment";
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: "notifications",
  templateUrl: "./notifications.component.html"
})
export class NotificationsComponent implements OnInit, OnDestroy {
  notifications$: any;
  user: User;
  notificationsCount$: Observable<number>;

  constructor(
    private authService: AuthService,
    public snackBar: MatSnackBar,
    private notificationService: NotificationService
  ) { }

  ngOnInit(): void {
    this.authService.user$.subscribe((user: User) => {
      this.user = user;
      this.notifications$ = this.notificationService
        .list(this.user.emailHash, 100)
        .snapshotChanges()
        .pipe(
          map(changes =>
            changes.map(c => {
              let val = c.payload.val();
              return <FirebaseNotification>{ Key: c.payload.key, Since: moment.utc(val.Date).fromNow(), ...val };
            })
          )
      );
      this.notificationsCount$ = this.notificationService.count(this.user.emailHash);
    });
  }

  markAsRead(notif: FirebaseNotification): void {
    if (this.user)
      this.notificationService.remove(this.user.emailHash, notif.Key);
  }

  markAllAsRead(): void {
    if (this.user)
      this.notificationService.removeAll(this.user.emailHash);
  }

  ngOnDestroy() { }
}
