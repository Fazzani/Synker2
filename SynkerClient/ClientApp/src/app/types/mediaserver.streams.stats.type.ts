export class MediaServerStreamsStats {
  public Streams: { [key: string]: Stream; };
}
export class Stream {
  public publisher: Publisher;
  public subscribers: Subscriber[];
}
export class Publisher {
  public app: string;
  public stream: string;
  public clientId: string;
  public connectCreated: Date;
  public bytes: number;
  public ip: string;
  public audio: Audio;
  public video: Video;
}
export class Audio {
  public codec: string;
  public profile: string;
  public samplerate: number;
  public channels: number;
}
export class Video {
  public codec: string;
  public width: number;
  public height: number;
  public profile: string;
  public level: number;
  public fps: number;
}
export class Subscriber {
  public app: string;
  public stream: string;
  public clientId: string;
  public connectCreated: Date;
  public bytes: number;
  public ip: string;
  public protocol: string;
}
