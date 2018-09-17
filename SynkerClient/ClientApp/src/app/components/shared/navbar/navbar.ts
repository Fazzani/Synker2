import { Component, NgModule, OnInit, OnDestroy, Output, EventEmitter } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule, MatMenuModule } from "@angular/material";
import { RouterModule } from "@angular/router";
import { AppModuleMaterialModule } from "../../../app.module.material.module";
import { User } from "../../../types/auth.type";
import { AuthService } from "../../../services/auth/auth.service";
import { BehaviorSubject, Subscription, Observable } from "rxjs";
import { EqualValidator } from "../../../directives/equal-validator.directive";
import { MessageService } from "../../../services/message/message.service";
import { Message, MessageStatus } from "../../../types/message.type";
import { PagedResult } from "../../../types/common.type";
import { AuthorizedRouteGuard } from "../../../services/auth/authorizedRouteGuard.service";
import { InitAppService } from "../../../services/initApp/InitAppService";
import { AboutApplication } from "../../../types/aboutApplication.type";
import { NotificationService } from "../../../services/notification/notification.service";
import FirebaseNotification from "../../../types/firebase.notification.type";

@Component({
  selector: "app-navbar",
  templateUrl: "./navbar.html",
  styleUrls: ["./navbar.scss"]
})
export class NavBar implements OnInit, OnDestroy {
  aboutApp: AboutApplication;
  isAuthenticated: BehaviorSubject<boolean>;
  user: BehaviorSubject<User>;
  userSubscription: Subscription;
  messages: PagedResult<Message>;
  notifications$: Observable<FirebaseNotification[]>;
  @Output() onThemeChanged = new EventEmitter();
  @Output() onWebPushClicked = new EventEmitter();
  notificationsCount$: Observable<number>;

  constructor(private authService: AuthService, private messageService: MessageService, public authorizedGuard: AuthorizedRouteGuard,
    private initAppService: InitAppService, private notificationService: NotificationService) {
    this.isAuthenticated = this.authService.authenticated;
    this.user = this.authService.user;
    this.authService.connect();
    this.aboutApp = this.initAppService.about;
  }

  ngOnInit(): void {
    this.userSubscription = this.user.subscribe(user => {
      if (user != undefined) {
        console.log(`User ${user.firstName} is authenticated...`);
        //this.messageService.listByStatus([MessageStatus.None, MessageStatus.NotReaded], 0, 10).subscribe(msg => {
        //  this.messages = msg;
        //});

        this.notifications$ = this.notificationService.list(5).valueChanges();
        this.notificationsCount$ = this.notificationService.count();
      }
    });
  }

  onSetTheme(theme) {
    this.onThemeChanged.emit(theme);
  }

  onClickWebPush() {
    this.onWebPushClicked.emit();
  }

  signout(): void {
    this.isAuthenticated.next(false);
    this.authService.signout();
  }

  ngOnDestroy(): void {
    this.userSubscription.unsubscribe();
  }
}

@NgModule({
  imports: [MatButtonModule, MatMenuModule, RouterModule, FormsModule, AppModuleMaterialModule, CommonModule, ReactiveFormsModule],
  exports: [NavBar, EqualValidator],
  declarations: [NavBar, EqualValidator]
})
export class NavBarModule { }
