import { Tvg } from "./media.type";
import { ISelectable } from "./common.type";

/**
 * mediaRef model
 * @description mediaRef Model.
 */
export class mediaRef implements ISelectable {
  constructor(name: string, culture: string) {
    this.displayNames = Array<string>(name);
    this.cultures = Array<string>(culture);
  }
  selected: boolean;
  displayNames: string[];
  tvg: Tvg;
  groups: string[];
  cultures: string[];
  mediaType: MediaTypes;
  id: string;
  defaultSite: string;
}

export enum MediaTypes {
  Video,
  Audio
}
