
import {filter, distinctUntilChanged, debounceTime} from 'rxjs/operators';
import { Component, OnInit, OnDestroy, HostBinding } from "@angular/core";
import "hammerjs";
import "./app.component.css";
import { NotificationService } from "../../services/notification/notification.service";
import { MatSnackBar } from "@angular/material";
import { CommonService, Constants } from "../../services/common/common.service";
import { AuthService } from "../../services/auth/auth.service";
import { Router } from "@angular/router";
import { Exception } from "../../types/common.type";
import * as variables from "../../variables";
import { OverlayContainer } from "@angular/cdk/overlay";

@Component({
  selector: "app",
  templateUrl: "./app.component.html"
})
export class AppComponent implements OnInit, OnDestroy {
  @HostBinding("class") componentCssClass;
  color = "primary";
  objLoaderStatus: boolean;

  constructor(
    private notifService: NotificationService,
    public snackBar: MatSnackBar,
    private commonService: CommonService,
    private authService: AuthService,
    private router: Router,
    private overlayContainer: OverlayContainer
  ) {
    this.objLoaderStatus = false;

    // this.notifService.messages.subscribe(
    //   m => {
    //     console.log("new message ", m);
    //     this.commonService.info(`New message ${m.messageType}`, m.content);
    //   },
    //   error => console.warn(error)
    // );
  }

  ngOnInit() {
    this.setTheme(localStorage.getItem(Constants.ThemeKey) || "dark-theme");

    this.authService
      .isAuthenticated().pipe(
      distinctUntilChanged())
      .subscribe(isAuth => {
        console.log(
          `----------- JwtInterceptor 401 isAuth = ${isAuth} current url ${this.router.routerState.snapshot.url} this.authService.redirectUrl: ${
            this.authService.redirectUrl
          }`
        );
        if (
          !isAuth &&
          this.router.routerState.snapshot.url != "" &&
          (this.router.routerState.snapshot.url != variables.SIGN_IN_URL && this.router.routerState.snapshot.url != variables.REGISTER_URL)
        ) {
          this.authService.redirectUrl = this.router.routerState.snapshot.url == variables.SIGN_IN_URL ? "/home" : this.router.routerState.snapshot.url;
          this.router.navigate(["signin"]);
        }
      });

    this.authService.authenticated.pipe(distinctUntilChanged()).subscribe(isAuth => {
      if (!isAuth && this.router.url != variables.SIGN_IN_URL && this.router.url != variables.REGISTER_URL) {
        this.authService.redirectUrl = this.router.routerState.snapshot.url;
        this.router.navigate(["signin"]);
      }
    });

    this.commonService.loaderStatus.pipe(
      distinctUntilChanged(),
      debounceTime(2000),)
      .subscribe((val: boolean) => {
        console.log("new loader data : ", val);
        this.objLoaderStatus = val;
      });

    this.commonService.error.pipe(
      distinctUntilChanged(),
      filter(err => err != null),)
      .subscribe((err: Exception) => {
        this.commonService.displayError(err.message, err.title);
      });
  }

  handleThemeChanged(themeName: string): void {
    this.setTheme(themeName);
    localStorage.setItem(Constants.ThemeKey, themeName);
  }

  setTheme(theme: string): void {
    this.componentCssClass = theme;
    const overlayContainerClasses = this.overlayContainer.getContainerElement().classList;
    const themeClassesToRemove: string[] = Array.from(Constants.ThemesList).filter((item: string) => item.includes("-theme"));
    if (themeClassesToRemove.length) {
      overlayContainerClasses.remove(...themeClassesToRemove);
    }
    overlayContainerClasses.add(theme);
  }

  ngOnDestroy(): void {}
}
