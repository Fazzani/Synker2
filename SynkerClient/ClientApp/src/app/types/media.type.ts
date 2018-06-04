import { ISelectable } from "./common.type";

/**
 * Tvg media (channel)
 * 
 * @export
 * @interface TvgMedia
 */
export class TvgMedia implements ISelectable {

    selected: boolean;
    id: string;
    name: string;
    displayName: string;
    position: number;
    enabled: boolean;
    mediaType: MediaType;
    url: string;
    lang: string;
    urls: Array<string>;
    tvg?: Tvg;
    isValid: boolean;
    startLineHeader?: string;
    group: string;
    mediaGroup: MediaGroup;
}

export enum MediaType {
    LiveTv = 0,
    Radio = 1,
    Video = 2,
    Audio = 3,
    Other = 4
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

export class MediaGroup {
    public name: string;
    public disabled: boolean;
    public position: number;
    public matchingMediaPattern: string;
}
