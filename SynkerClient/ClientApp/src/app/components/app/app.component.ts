import { Component, OnInit, OnDestroy, HostBinding } from "@angular/core";
import { filter, distinctUntilChanged, debounceTime } from "rxjs/operators";
import { Router, NavigationStart, NavigationCancel, NavigationEnd, NavigationError } from "@angular/router";
import "hammerjs";
import "./app.component.css";
import { MatSnackBar } from "@angular/material";
import { CommonService, Constants } from "../../services/common/common.service";
//import { AuthService } from "../../services/auth/auth.service";
import { Exception } from "../../types/common.type";
//import * as variables from "../../variables";
import { OverlayContainer } from "@angular/cdk/overlay";
import { SwUpdate, SwPush } from "@angular/service-worker";
import { DeviceService } from "../../services/device/device.service";
import { OAuthService, JwksValidationHandler, AuthConfig, OAuthEvent } from 'angular-oauth2-oidc';
import { environment } from '../../../environments/environment';
import { BehaviorSubject } from 'rxjs';
import { User } from "../../types/auth.type";

@Component({
  selector: "app",
  templateUrl: "./app.component.html"
})
export class AppComponent implements OnInit, OnDestroy {
  @HostBinding("class") componentCssClass;
  color = "primary";
  objLoaderStatus: boolean;
  loading: boolean;
  readonly VAPID_PUBLIC_KEY = "BBxqxISU8686kkJSKc_DQdjHWQUG6THMmKai6QKHDS_RuA_dYCR9EwaNpWQzhRUViLpV_ttEmiJfxNvHjE7F7Rc";
  user = new BehaviorSubject<User>(null);
  isAuthenticated: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  constructor(
    private swPush: SwPush,
    private swUpdate: SwUpdate,
    public snackBar: MatSnackBar,
    private deviceService: DeviceService,
    private commonService: CommonService,
    private router: Router,
    private overlayContainer: OverlayContainer,
    private oauthService: OAuthService
  ) {
    this.objLoaderStatus = false;

    this.loading = true;
    this.configureWithNewConfigApi();
  }

  private configureWithNewConfigApi() {
    this.oauthService.configure(<AuthConfig>{
      clientId: environment.idp.client_id,
      issuer: environment.idp.authority,
      redirectUri: environment.idp.redirect_uri,
      scope: environment.idp.scope
    });
    this.oauthService.tokenValidationHandler = new JwksValidationHandler();
    this.oauthService.showDebugInformation = true;
    this.oauthService.loadDiscoveryDocument().then((doc) => {
      this.oauthService.tryLogin()
        .catch(err => {
          console.error(err);
        })
        .then(() => {
          if (!this.oauthService.hasValidAccessToken()) {
            this.oauthService.initImplicitFlow();
          } else {
            this.oauthService.loadUserProfile().then((userProfile: any) => 
              this.user.next(User.FromUserProfile(userProfile))
            )
          }
        });
    });
    this.oauthService.setupAutomaticSilentRefresh();
  }

  ngOnInit() {
    this.setTheme(localStorage.getItem(Constants.ThemeKey) || "dark-theme");
    this.oauthService.events.subscribe((authEvent: OAuthEvent) => {

      console.debug('oauth/oidc event', authEvent);
      if (authEvent.type == 'user_profile_loaded' && this.user.getValue() == null) {
        this.isAuthenticated.next(true);

        // TODO: Share the some user && adding user photo, id to scope
        // TODO: gérer le cas de déconnexion (expired_token, auth_failed, signout, etc...)
        // TODO: explicit signout

        this.oauthService.loadUserProfile().then((userProfile: any) => {
          //{"name":"Admin Smith","given_name":"Admin","family_name":"Administrator","email":"admin@email.com","website":"http://admin.com","sub":"88421153"}
          this.user.next(<User>{ email: userProfile.email, firstName: userProfile.given_name, lastName: userProfile.name, photo: userProfile.picture })
        });
      } else if (authEvent.type == 'logout' || authEvent.type === 'token_expires') {
        this.isAuthenticated.next(false);
      }
    });

    //this.authService
    //  .isAuthenticated()
    //  .pipe(distinctUntilChanged())
    //  .subscribe(isAuth => {
    //    console.log(
    //      `----------- JwtInterceptor 401 isAuth = ${isAuth} current url ${this.router.routerState.snapshot.url} this.authService.redirectUrl: ${
    //        this.authService.redirectUrl
    //      }`
    //    );
    //    if (
    //      !isAuth &&
    //      this.router.routerState.snapshot.url != "" &&
    //      (this.router.routerState.snapshot.url != variables.SIGN_IN_URL && this.router.routerState.snapshot.url != variables.REGISTER_URL)
    //    ) {
    //      this.authService.redirectUrl = this.router.routerState.snapshot.url == variables.SIGN_IN_URL ? "/home" : this.router.routerState.snapshot.url;
    //      this.router.navigate(["signin"]);
    //    }
    //  });

    //this.authService.authenticated.pipe(distinctUntilChanged()).subscribe(isAuth => {
    //  if (!isAuth && this.router.url != variables.SIGN_IN_URL && this.router.url != variables.REGISTER_URL) {
    //    this.authService.redirectUrl = this.router.routerState.snapshot.url;
    //    this.router.navigate(["signin"]);
    //  }
    //});

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
        serverPublicKey: this.VAPID_PUBLIC_KEY
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
