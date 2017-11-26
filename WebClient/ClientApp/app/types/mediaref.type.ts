import { Tvg } from "./media.type";


/**
 * mediaRef model
 * @description mediaRef Model.
 */
export interface mediaRef {
    displayNames: Array<string>;
    tvg: Tvg;
    groups: Array<string>;
    cultures: Array<string>;
    mediaType: string;
    id: string;
}
