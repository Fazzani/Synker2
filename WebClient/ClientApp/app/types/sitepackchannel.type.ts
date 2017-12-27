import { ISelectable } from "./common.type";

/**
 * SitePack channel Model
 * @description SitePack channel Model.
 */
export class sitePackChannel implements ISelectable {
    id: string;
    update: string;
    site: string;
    site_id: string;
    xmltv_id: string;
    channel_name: string;
    country: string;
    displayNames: Array<string>;
    mediaType: SitePackMediaTypes;
    selected: boolean;
}

export enum SitePackMediaTypes {
    Channel,
    Radio
}