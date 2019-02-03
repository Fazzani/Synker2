import { TvgMedia } from "./media.type";
import { PlayerApi } from "./xtream.type";

/**
 * Message Type
 * @description Message type.
 */
export interface Playlist {
  UniqueId: string;
  UserId: Number;
  Freindlyname: string;
  Content: string;
  IsSynchronizable: boolean;
  SynkConfig: SynkConfig;
  Status: PlaylistStatus;
  TvgMedias: TvgMedia[];
}

export class SynkConfig {
  public synkLogos: boolean;
  public synkEpg: boolean;
  public synkGroup: SynkGroupEnum;
  public cleanName: boolean;
  public url: string;
  public provider: string;
  public notifcationTypeInsertedMedia: NotificationTypeEnum;
}
export enum SynkGroupEnum {
  None = 0,
  ByCountry = 1,
  ByLanguage = 2,
  Custom = 3
}

export enum PlaylistStatus {
  Enabled = 0,
  Disabled = 1
}

export enum Providers {
  m3u,
  tvlist,
  xtream
}

export enum NotificationTypeEnum {
  pushBrowser = 1,
  pushMobile = 2,
  email = 4,
  sms = 8
}
export class PlaylistModel {
  public id: number;
  public uniqueId: string;
  public userId: number;
  public freindlyname: string;
  public tvgMedias: TvgMedia[];
  public tvgSites: string[];
  public url: string;
  public createdDate: Date;
  public updatedDate: Date;
  public synkEpg: boolean;
  public synkGroup: SynkGroupEnum;
  public status: PlaylistStatus;
  public notifcationTypeInsertedMedia: NotificationTypeEnum;
  public synkLogos: boolean;
  public publicUrl: string;
  public publicId: string;
  public xtreamPlayerApi: PlayerApi;
  public isXtream: boolean;
  public importProvider: string;
  public tags: any;
  public static PROVIDERS: string[] = Object.keys(Providers).slice(Object.keys(Providers).length / 2);
  public static SYNKGROUP: string[] = Object.keys(SynkGroupEnum).slice(Object.keys(SynkGroupEnum).length / 2);
  public static STATUS: string[] = Object.keys(PlaylistStatus).slice(Object.keys(PlaylistStatus).length / 2);
}

export class PlaylistModelLive {
  public id: number;
  public uniqueId: string;
  public userId: number;
  public freindlyname: string;
  public tvgMedias: TvgMedia[];
  public tvgSites: string[];
  public url: string;
  public createdDate: Date;
  public updatedDate: Date;
  public synkEpg: boolean;
  public synkGroup: SynkGroupEnum;
  public status: PlaylistStatus;
  public notifcationTypeInsertedMedia: NotificationTypeEnum;
  public synkLogos: boolean;
  public publicUrl: string;
  public publicId: string;
  public xtreamPlayerApi: PlayerApi;
  public isXtream: boolean;
  public importProvider: string;
  public tags: any;
  public static PROVIDERS: string[] = Object.keys(Providers).slice(Object.keys(Providers).length / 2);
  public static SYNKGROUP: string[] = Object.keys(SynkGroupEnum).slice(Object.keys(SynkGroupEnum).length / 2);
  public isOnline: boolean;
  public mediaCount: number;
}

export class PlaylistPostModel {
  public freindlyname: string;
  public status: PlaylistStatus;
  public url: string;
  public synkEpg: boolean;
  public synkGroup: SynkGroupEnum;
  public synkLogos: boolean;
  public provider: string;
  public publicId: string;
  public notifcationTypeInsertedMedia: NotificationTypeEnum;
}
