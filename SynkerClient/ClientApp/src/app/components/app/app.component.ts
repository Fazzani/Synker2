import { Component, OnInit, OnDestroy, ViewContainerRef, HostBinding } from '@angular/core';
import 'hammerjs';
import './app.component.css'
import { NotificationService } from '../../services/notification/notification.service';
import { MatSnackBar } from '@angular/material';
import { CommonService } from '../../services/common/common.service';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';
import { Exception } from '../../types/common.type';
import * as variables from "../../variables";
import { OverlayContainer } from '@angular/cdk/overlay';

@Component({
  selector: 'app',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit, OnDestroy {

  @HostBinding('class') componentCssClass;
  color = 'primary';
  objLoaderStatus: boolean;

  constructor(private notifService: NotificationService, public snackBar: MatSnackBar, private commonService: CommonService,
    private authService: AuthService, private router: Router, vcr: ViewContainerRef, private overlayContainer: OverlayContainer) {

    this.objLoaderStatus = false;

    notifService.messages.subscribe(
      m => {
        console.log('new message ', m);
        this.commonService.info(`New message ${m.messageType}`, m.content);
      },
      error => console.warn(error));
  }

  ngOnInit() {

    this.setTheme("dark-theme");

    this.authService
      .isAuthenticated()
      .distinctUntilChanged()
      .subscribe(isAuth => {
        console.log(`----------- JwtInterceptor 401 isAuth = ${isAuth} current url ${this.router.routerState.snapshot.url} this.authService.redirectUrl: ${this.authService.redirectUrl}`);
        if (!isAuth && this.router.routerState.snapshot.url != "" && (this.router.routerState.snapshot.url != variables.SIGN_IN_URL && this.router.routerState.snapshot.url != variables.REGISTER_URL)) {
          this.authService.redirectUrl = this.router.routerState.snapshot.url == variables.SIGN_IN_URL ? "/home" : this.router.routerState.snapshot.url;
          this.router.navigate(['signin']);
        }
      });

    this.authService.authenticated.distinctUntilChanged().subscribe(isAuth => {
      if (!isAuth && this.router.url != variables.SIGN_IN_URL && this.router.url != variables.REGISTER_URL) {
        this.authService.redirectUrl = this.router.routerState.snapshot.url;
        this.router.navigate(['signin']);
      }
    });


    this.commonService.loaderStatus.distinctUntilChanged().debounceTime(2000).subscribe((val: boolean) => {
      console.log('new loader data : ', val);
      this.objLoaderStatus = val;
    });

    this.commonService.error.distinctUntilChanged().filter(err => err != null).subscribe((err: Exception) => {
      this.commonService.displayError(err.message, err.title);
    });
  }

  handleThemeChanged(event): void {
    this.setTheme(event);
  }

  setTheme(theme: string): void {
    this.componentCssClass = theme;
    const overlayContainerClasses = this.overlayContainer.getContainerElement().classList;
    const classList = ["default-theme", "dark-theme", "light-theme"];
    const themeClassesToRemove: string[] = Array.from(classList).filter((item: string) => item.includes('-theme'));
    if (themeClassesToRemove.length) {
      overlayContainerClasses.remove(...themeClassesToRemove);
    }
    overlayContainerClasses.add(theme);
  }

  ngOnDestroy(): void {
  }
}
