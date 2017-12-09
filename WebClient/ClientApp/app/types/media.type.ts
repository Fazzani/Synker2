﻿import { ISelectable } from "./common.type";

/**
 * Tvg media (channel)
 * 
 * @export
 * @interface TvgMedia
 */
export interface TvgMedia extends ISelectable {
    id: string;
    name: string;
    displayName: string;
    group?: string;
    position: number;
    enabled: boolean;
    mediaType: number;
    url: string;
    lang: string;
    urls: Array<string>;
    tvg?: Tvg;
    isValid: boolean;
    startLineHeader?: string;
}

/**
 * Tvg channel field
 * 
 * @export
 * @interface Tvg
 */
export interface Tvg extends ISelectable {
    id: string;
    logo?: string;
    name: string;
    tvgIdentify: string;
    shift: string;
    audio_track: string;
    aspect_ratio: string;
    tvgSource: TvgSource;
}

export class Culture {
    public code: string;
    public country: string;
}

export class TvgSource extends Culture {
    public site: string;
}
