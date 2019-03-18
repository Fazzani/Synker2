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
    authority: 'https://idp.synker.ovh',
    client_id: 'synkerclient',
    redirect_uri: 'https://synker.ovh/auth-callback',
    post_logout_redirect_uri: 'https://synker.ovh/',
    response_type: "id_token token",
    scope: "openid profile email synkerapi.full_access",
    filterProtocolClaims: true,
    loadUserInfo: true,
    automaticSilentRenew: true,
    silent_redirect_uri: 'https://synker.ovh/silent-refresh.html'
  }
};
