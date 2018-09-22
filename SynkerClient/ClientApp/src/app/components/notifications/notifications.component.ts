import { Component, OnInit, OnDestroy } from "@angular/core";
import { MatSnackBar } from "@angular/material";
import { ActivatedRoute } from "@angular/router";
import { NotificationService } from "../../services/notification/notification.service";
import { Observable } from "rxjs";
import { AuthService } from "../../services/auth/auth.service";
import FirebaseNotification from "../../types/firebase.type";
import { User } from "../../types/auth.type";
import { map } from "rxjs/operators";

@Component({
  selector: "notifications",
  templateUrl: "./notifications.component.html"
})
export class NotificationsComponent implements OnInit, OnDestroy {
  notifications$: any;
  user: User;

  constructor(
    private route: ActivatedRoute,
    private authService: AuthService,
    public snackBar: MatSnackBar,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.authService.user.subscribe(user => {
      this.user = user;
      this.notifications$ = this.notificationService
        .list(user.id, 100)
        .snapshotChanges()
        .pipe(map(changes => changes.map(c => <FirebaseNotification>{ Key: c.payload.key, ...c.payload.val() })));
    });
  }

  markAsRead(notif: FirebaseNotification): void {
    this.notificationService.remove(this.user.id, notif.Key);
  }

  markAllAsRead(): void {
    this.notificationService.removeAll(this.user.id);
  }

  ngOnDestroy() {}
}
