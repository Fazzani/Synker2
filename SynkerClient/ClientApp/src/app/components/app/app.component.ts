import { Component, OnInit, OnDestroy, HostBinding } from "@angular/core";
import { filter, distinctUntilChanged, debounceTime } from "rxjs/operators";
import { Router, NavigationStart, NavigationCancel, NavigationEnd, NavigationError } from "@angular/router";
import "hammerjs";
import "./app.component.css";
import { MatSnackBar } from "@angular/material";
import { CommonService, Constants } from "../../services/common/common.service";
import { Exception } from "../../types/common.type";
import { OverlayContainer } from "@angular/cdk/overlay";
import { SwUpdate, SwPush } from "@angular/service-worker";
import { DeviceService } from "../../services/device/device.service";
import { OAuthService, JwksValidationHandler, AuthConfig } from 'angular-oauth2-oidc';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { User } from "../../types/auth.type";
import { AuthService } from '../../services/auth/auth.service';
import { VAPID_PUBLIC_KEY } from '../../variables';

@Component({
  selector: "app",
  templateUrl: "./app.component.html"
})
export class AppComponent implements OnInit, OnDestroy {
  @HostBinding("class") componentCssClass;
  color = "primary";
  objLoaderStatus: boolean;
  loading: boolean;
  user: Observable<User>;
  isAuthenticated: Observable<boolean>;
  isDoneLoading: Observable<boolean>;
  canActivateProtectedRoutes: Observable<boolean>;

  constructor(
    private swPush: SwPush,
    private swUpdate: SwUpdate,
    public snackBar: MatSnackBar,
    private deviceService: DeviceService,
    private commonService: CommonService,
    private router: Router,
    private overlayContainer: OverlayContainer,
    private oauthService: OAuthService,
    private authService: AuthService
  ) {
    this.objLoaderStatus = false;
    this.isAuthenticated = this.authService.isAuthenticated$;
    this.isDoneLoading = this.authService.isDoneLoading$;
    this.user = this.authService.user$;
    this.canActivateProtectedRoutes = this.authService.canActivateProtectedRoutes$;

    this.loading = true;
    this.configureWithNewConfigApi();
  }

  private configureWithNewConfigApi() {
    this.oauthService.configure(<AuthConfig>environment.idp);
    this.oauthService.tokenValidationHandler = new JwksValidationHandler();
    this.oauthService.showDebugInformation = true;

    this.authService.runInitialLoginSequence();
  }

  ngOnInit() {
    this.setTheme(localStorage.getItem(Constants.ThemeKey) || "dark-theme");
   
    this.commonService.loaderStatus
      .pipe(
        distinctUntilChanged(),
        debounceTime(2000)
      )
      .subscribe((val: boolean) => {
        console.log("new loader data : ", val);
        this.objLoaderStatus = val;
      });

    this.commonService.error
      .pipe(
        distinctUntilChanged(),
        filter(err => err != null)
      )
      .subscribe((err: Exception) => {
        this.commonService.displayError(err.message, err.title);
      });

    //angular-service-worker for progressive app
    // auto refresh when new version is available
    this.swUpdate.available.subscribe(event => {
      console.log("[App] Update available: current version is", event.current, "available version is", event.available);
      let snackBarRef = this.snackBar.open("Newer version of the app is available", "Refresh");

      snackBarRef.onAction().subscribe(() => {
        window.location.reload();
      });
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

  handelSubscribeToWebPush(event: any) {
    this.swPush
      .requestSubscription({
        serverPublicKey: VAPID_PUBLIC_KEY
      })
      .then(sub => {
        console.log('PushSubscription: ', JSON.stringify(sub));
        return this.deviceService.create(sub).subscribe();
      })
      .catch(err => console.error("Could not subscribe to notifications", err));
  }

  ngOnDestroy(): void { }

  ngAfterViewInit() {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationStart) {
        this.loading = true;
      } else if (event instanceof NavigationEnd || event instanceof NavigationError || event instanceof NavigationCancel) {
        setTimeout(() => {
          this.loading = false;
        }, 100);
      }
    });
  }
}
