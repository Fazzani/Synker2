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

export enum MatchingTvgSiteTypeEnum {
  TvgSiteInMedia = 0,
  TvgSitePlaylist,
  All
}

export class MatchTvgFormModel {
  constructor() {
    this.minScore = 0.5;
    this.overrideTvg = true;
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
  public static MatchingTvgSiteTYPES: string[] = Object.keys(MatchingTvgSiteTypeEnum).slice(Object.keys(MatchingTvgSiteTypeEnum).length / 2);
}
