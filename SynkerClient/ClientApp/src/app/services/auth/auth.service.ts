import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { OAuthErrorEvent, OAuthService } from 'angular-oauth2-oidc';
import { BehaviorSubject, combineLatest, Observable, ReplaySubject } from 'rxjs';
import { filter, map } from 'rxjs/operators';
import { User } from '../../types/auth.type';
import { CommonService } from '../common/common.service';
import { Exception } from '../../types/common.type';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private isAuthenticatedSubject$ = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject$.asObservable();

  private isDoneLoadingSubject$ = new ReplaySubject<boolean>();
  public isDoneLoading$ = this.isDoneLoadingSubject$.asObservable();
  public user$ = new BehaviorSubject<User>(null);

  constructor(private oauthService: OAuthService, private commonService: CommonService, private router: Router) {
    // Useful for debugging:
    this.oauthService.events.subscribe(event => {
      if (event instanceof OAuthErrorEvent) {
        this.commonService.error.next(<Exception>{ title: event.type, message: JSON.stringify(event.reason) });
      } else {
        console.warn('OAuth Events => ', event);
      }
    });

    // This is tricky, as it might cause race conditions (where access_token is set in another
    // tab before everything is said and done there.
    // TODO: Improve this setup.
    window.addEventListener('storage', event => {
      // The `key` is `null` if the event was caused by `.clear()`
      if (event.key !== 'access_token' && event.key !== null) {
        return;
      }

      console.warn('Noticed changes to access_token (most likely from another tab), updating isAuthenticated');
      this.isAuthenticatedSubject$.next(this.oauthService.hasValidAccessToken());

      if (!this.oauthService.hasValidAccessToken()) {
        this.navigateToLoginPage();
      }
    });

    this.oauthService.events.subscribe(_ => {
      this.isAuthenticatedSubject$.next(this.oauthService.hasValidAccessToken());
    });

    this.oauthService.events
      .pipe(filter(e => ['token_received', 'discovery_document_loaded'].includes(e.type)))
      .subscribe(e => {
        if (this.oauthService.hasValidAccessToken()) {
          this.oauthService.loadUserProfile()
            .then((userProfile: any) => this.user$.next(User.FromUserProfile(userProfile)));
        }
      });

    //silent_refresh_timeout
    //token_expires
    this.oauthService.events
      .pipe(filter(e => ['session_terminated', 'session_error', 'token_validation_error']
      .includes(e.type))).subscribe(e => this.navigateToLoginPage());

    //this.oauthService.setupAutomaticSilentRefresh();
  }

  public runInitialLoginSequence(): Promise<void> {
    if (location.hash) {
      console.log('Encountered hash fragment, plotting as table...');
      console.table(
        location.hash
          .substr(1)
          .split('&')
          .map(kvp => kvp.split('=')),
      );
    }

    // 0. LOAD CONFIG:
    // First we have to check to see how the IdServer is
    // currently configured:
    return (
      this.oauthService
        .loadDiscoveryDocument()

        // 1. HASH LOGIN:
        // Try to log in via hash fragment after redirect back
        // from IdServer from initImplicitFlow:
        .then(() => this.oauthService.tryLogin())

        .then(() => {
          if (this.oauthService.hasValidAccessToken()) {
            return Promise.resolve();
          }

          this.oauthService.initImplicitFlow();
          //
          // Instead, we'll now do this:
          console.warn('User interaction is needed to log in, we will wait for the user to manually log in.');
          return Promise.resolve();
        })

        .then(() => {
          this.isDoneLoadingSubject$.next(true);

          if (this.oauthService.state && this.oauthService.state !== 'undefined' && this.oauthService.state !== 'null') {
            console.log('There was state, so we are sending you to: ' + this.oauthService.state);
            this.router.navigateByUrl(this.oauthService.state);
          }
        })
        .catch(() => this.isDoneLoadingSubject$.next(true))
    );
  }

  /**
   * Publishes `true` if and only if (a) all the asynchronous initial
   * login calls have completed or errorred, and (b) the user ended up
   * being authenticated.
   *
   * In essence, it combines:
   *
   * - the latest known state of whether the user is authorized
   * - whether the ajax calls for initial log in have all been done
   */
  public canActivateProtectedRoutes$: Observable<boolean> = combineLatest(this.isAuthenticated$, this.isDoneLoading$).pipe(map(values => values.every(b => b)));

  private navigateToLoginPage() {
    // TODO: Remember current URL
    this.router.navigateByUrl('/should-login');
  }

  public login(targetUrl?: string) {
    this.oauthService.initImplicitFlow(encodeURIComponent(targetUrl || this.router.url));
  }

  public logout(noRedirectToLogoutUrl?: boolean) {
    this.oauthService.logOut(noRedirectToLogoutUrl);
  }
  //public refresh() { this.oauthService.silentRefresh(); }
  public hasValidToken() {
    return this.oauthService.hasValidAccessToken();
  }

  // These normally won't be exposed from a service like this, but
  // for debugging it makes sense.
  public get accessToken() {
    return this.oauthService.getAccessToken();
  }
  public get identityClaims() {
    return this.oauthService.getIdentityClaims();
  }
  public get idToken() {
    return this.oauthService.getIdToken();
  }
  public get logoutUrl() {
    return this.oauthService.logoutUrl;
  }

  hasRole(role: string): boolean | Observable<boolean> | Promise<boolean> {
    const user = this.user$.getValue();
    if (user && user.roles) {
      return user.roles.indexOf(role) > -1;
    }
    return false;
  }
}
