﻿import { TvgMedia } from "./media.type";
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
    public cron: string;
    public synkLogos: boolean;
    public synkEpg: boolean;
    public synkGroup: SynkGroupEnum;
    public cleanName: boolean;
    public url: string;
    public provider: string;
}
export enum SynkGroupEnum {
    none = 0,
    byCountry = 1,
    byLanguage = 2,
    custom = 3
}

export enum PlaylistStatus {
    enabled = 0,
    disabled = 1
}

export class PlaylistModel {
    public userId: number;
    public freindlyname: string;
    public cron: string;
    public status: PlaylistStatus;
    public tvgMedias: TvgMedia[];
    public tvgSites: string[];
    public url: string;
    public createdDate: Date;
    public updatedDate: Date;
    public id: number;
    public uniqueId: string;
    public synkEpg: boolean;
    public synkGroup: SynkGroupEnum;
    public synkLogos: boolean;
    public publicUrl: string;
    public publicId: string;
    public xtreamPlayerApi: PlayerApi;
    public isXtream: boolean;
}

export class PlaylistPostModel {
    public freindlyname: string;
    public cron: string;
    public status: PlaylistStatus;
    public url: string;
    public synkEpg: boolean;
    public synkGroup: SynkGroupEnum;
    public synkLogos: boolean;
    public provider: string;
    public publicId: string;
}

export enum Providers {
    m3u,
    tvlist
}

