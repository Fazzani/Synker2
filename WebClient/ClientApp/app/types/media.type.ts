/**
 * Tvg media (channel)
 * 
 * @export
 * @interface TvgMedia
 */
export interface TvgMedia {
    id: string;
    name: string;
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
export interface Tvg {
    id: string;
    logo?: string;
    name: string;
    tvgIdentify: string;
    Shift: string;
    Audio_track: string;
    Aspect_ratio: string;
}