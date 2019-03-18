// The file contents for the current environment will overwrite these during build.
// The build system defaults to the dev environment which uses `environment.ts`, but if you do
// `ng build --env=prod` then `environment.prod.ts` will be used instead.
// The list of which env maps to which file can be found in `.angular-cli.json`.

export const environment = {
  production: false,
  base_url: "/",
  base_hub_url: "//localhost:56800/",
  base_api_url: "//localhost:56800/api/v1/",
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
    authority: 'http://localhost:5000',
    client_id: 'synkerclient',
    redirect_uri: 'http://localhost:56810/auth-callback',
    post_logout_redirect_uri: 'http://localhost:56810/',
    response_type: "id_token token",
    scope: "openid profile synkerapi.full_access",
    filterProtocolClaims: true,
    loadUserInfo: true,
    automaticSilentRenew: true,
    silent_redirect_uri: 'http://localhost:56810/silent-refresh.html'
  }
}
