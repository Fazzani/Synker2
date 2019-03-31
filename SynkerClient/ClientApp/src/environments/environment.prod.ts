import { OAuthModuleConfig } from 'angular-oauth2-oidc';

export const environment = {
  production: true,
  base_url: "/",
  base_hub_url: "//api.synker.ovh/",
  base_api_url: "//api.synker.ovh/api/v1/",
  base_proxy_url: "//servermedia.synker.ovh",
  firebaseConfig: {
    secret: "SMDRzX3kVsP9Tci0ADhOYo62Ty4bMkEDm3qiihlL",
    apiKey: "AIzaSyAgozGrBpxU01ezugBKXMJMPZ184BzU6JY",
    authDomain: "holo-970f4.firebaseapp.com",
    databaseURL: "https://holo-970f4.firebaseio.com",
    projectId: "holo-970f4",
    storageBucket: "holo-970f4.appspot.com",
    messagingSenderId: "685189543315"
  },
  idp: {
    issuer: 'https://idp.synker.ovh',
    clientId: 'synkerclient',
    redirectUri: 'https://synker.ovh/index.html',
    postLogoutRedirectUri: 'https://synker.ovh/',
    responseType: "id_token token",
    scope: "openid profile synkerapi.full_access synkerapi.read_only",
    filterProtocolClaims: true,
    loadUserInfo: true,
    automaticSilentRenew: false,
    silent_redirect_uri: 'https://synker.ovh/silent-refresh.html',
    sessionChecksEnabled: true,
    clearHashAfterLogin: false // https://github.com/manfredsteyer/angular-oauth2-oidc/issues/457#issuecomment-431807040,
  },
  authModuleConfig: {
    // Inject "Authorization: Bearer ..." header for these APIs:
    resourceServer: {
      allowedUrls: ["//api.synker.ovh/api"],
      sendAccessToken: true
    }
  }
};
export const authModuleConfig: OAuthModuleConfig = environment.authModuleConfig;
