import { Tvg } from "./media.type";
import { ISelectable } from "./common.type";


/**
 * mediaRef model
 * @description mediaRef Model.
 */
export interface mediaRef extends ISelectable {
    displayNames: Array<string>;
    tvg: Tvg;
    groups: Array<string>;
    cultures: Array<string>;
    mediaType: string;
    id: string;
}
