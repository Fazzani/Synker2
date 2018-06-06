export class MatchTvgPostModel {
  constructor() {
    this.minScore = 0.5;
  }
  public mediaName: string;
  public tvgSites: Array<string>;
  public country: string;
  /**
   * Minimum scoring matching
   */
  public minScore: number;
}

export class MatchTvgFormModel {
  constructor() {
    this.minScore = 0.5;
    this.overrideTvg = false;
    this.matchAll = false;
    this.matchingTvgSiteType = MatchingTvgSiteTypeEnum.TvgSiteInMedia;
  }
  /**
   * Match all medias or only selected
   */
  public matchingTvgSiteType: MatchingTvgSiteTypeEnum;
  /**
   * Match all medias or only selected
   */
  public matchAll: boolean;
  /**
   * Force override Tvg
   */
  public overrideTvg: boolean;
  /**
   * Match Country
   */
  public matchCountry: boolean;
  /**
   * Minimum scoring matching
   */
  public minScore: number;
}

export enum MatchingTvgSiteTypeEnum {
  TvgSiteInMedia = 0,
  TvgSitePlaylist,
  All
}
