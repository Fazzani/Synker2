export class MediaServerOptions {
  public Host: string;
  public Rtmp: number;
  public Port: number;
  public IsSecure: boolean;
  public BasicAuthApiOptions: BasicAuthOptions;
  public Auth: AuthOptions;

  static Default: MediaServerOptions = <MediaServerOptions>{
    Rtmp: 1935,
    Host: "servermedia.synker.ovh",
    IsSecure: false,
    Auth: undefined,
    Port: 80
  };
}
export class BasicAuthOptions {
  public UserName: string;
  public Password: string;
}
export class AuthOptions {
  public Play: boolean;
  public Publish: boolean;
  public Secret: string;
}

