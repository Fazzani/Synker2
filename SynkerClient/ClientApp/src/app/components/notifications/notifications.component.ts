import { Component, OnInit, OnDestroy } from "@angular/core";
import { MatSnackBar } from "@angular/material";
import { ActivatedRoute } from "@angular/router";
import { NotificationService } from "../../services/notification/notification.service";
import { Observable } from "rxjs";
import { AuthService } from "../../services/auth/auth.service";
import FirebaseNotification from "../../types/firebase.type";

@Component({
  selector: "notifications",
  templateUrl: "./notifications.component.html"
})
export class NotificationsComponent implements OnInit, OnDestroy {
  notifications$: Observable<FirebaseNotification[]>;

  constructor(private route: ActivatedRoute, private authService: AuthService, public snackBar: MatSnackBar,
    private notificationService: NotificationService) { }

  ngOnInit(): void {
    this.authService.user.subscribe(user => {
      this.notifications$ = this.notificationService.list(user.id, 100).valueChanges();
    });
  }

  markAsRead(notif: FirebaseNotification): void {

  }

  markAllAsRead(): void {
  }

  ngOnDestroy() { }
}
