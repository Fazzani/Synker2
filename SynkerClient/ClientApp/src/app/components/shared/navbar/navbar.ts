import { Component, NgModule, OnInit, OnDestroy, Output, EventEmitter, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule, MatMenuModule } from "@angular/material";
import { RouterModule } from "@angular/router";
import { AppModuleMaterialModule } from "../../../app.module.material.module";
import { User } from "../../../types/auth.type";
import { Subscription, Observable, BehaviorSubject } from "rxjs";
import { EqualValidator } from "../../../directives/equal-validator.directive";
import { InitAppService } from "../../../services/initApp/InitAppService";
import { AboutApplication } from "../../../types/aboutApplication.type";
import { NotificationService } from "../../../services/notification/notification.service";
import FirebaseNotification from "../../../types/firebase.type";
import { AuthService } from '../../../services/auth/auth.service';
import { AdminRole } from '../../../variables';

@Component({
  selector: "app-navbar",
  templateUrl: "./navbar.html",
  styleUrls: ["./navbar.scss"]
})
export class NavBar implements OnInit, OnDestroy {
  aboutApp: AboutApplication;

  private _isAuthenticated: boolean;
  @Input()
  set isAuthenticated(isAuthenticated: boolean) {
    console.log(`isAuthenticated   =>>>>>  ${isAuthenticated}`);
    this._isAuthenticated = isAuthenticated;
  }
  get isAuthenticated(): boolean { return this._isAuthenticated; }

  private _user: User;
  @Input()
  set user(user: User) {
    this._user = user;
  }
  get user(): User { return this._user; }

  userSubscription: Subscription;
  isAdmin$: BehaviorSubject<boolean> = new BehaviorSubject(false);
  notifications$: Observable<FirebaseNotification[]>;
  @Output()
  onThemeChanged = new EventEmitter();
  @Output()
  onWebPushClicked = new EventEmitter();
  notificationsCount$: Observable<number>;

  //TODO: init from localStorage
  notificationOn: boolean = true;

  constructor(
    private authService: AuthService,
    private initAppService: InitAppService,
    private notificationService: NotificationService
  ) {

    this.aboutApp = this.initAppService.about;
  }

  ngOnInit(): void {
    this.userSubscription = this.authService.user$.subscribe((user: User) => {
      if (user != undefined) {
        console.log(`User ${user.firstName} is authenticated...${user.emailHash}`);
        //TODO: Notifications by email Or getting userId from Synker API
        this.notifications$ = this.notificationService.list(user.emailHash, 5).valueChanges();
        this.notificationsCount$ = this.notificationService.count(user.emailHash);
        this.isAdmin$.next(this.authService.hasRole(AdminRole) as boolean);
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
    this.authService.logout();
  }

  toggleNotification = () => {
    this.notificationOn = !this.notificationOn;
    //TODO disable push and save config to localStorage
  };

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
