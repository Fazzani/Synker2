import { Tvg } from "./media.type";
import { ISelectable } from "./common.type";


/**
 * mediaRef model
 * @description mediaRef Model.
 */
export interface mediaRef extends ISelectable {
    displayNames: string[];
    tvg: Tvg;
    groups: string[];
    cultures: string[];
    mediaType: MediaTypes;
    id: string;
}

export enum MediaTypes {
    Video,
    Audio 
}
