export class tvProgrammeReview {
  public lang: string;
  public type: string;
  public Value: string;
}
export class tvProgrammeStarrating {
  public value: string;
}

export class tvProgrammeRating {
  public value: string;
  public system: string;
}

export class tvProgrammeSubtitles {
  public origlanguage: Array<string>;
  public language: Array<string>;
  public type: string;
}
export class tvProgrammeAudio {
  public stereo: string;
}

export class tvProgrammeVideo {
  public aspect: string;
  public quality: string;
}

export class tvProgrammeIcon {
  public src: string;
}

export class tvProgrammeLength {
  public units: string;
  public Value: string;
}

export class tvProgrammeCategory {
  public lang: string;
  public Value: string;
}

export class keyword {
  public lang: string;
  public Value: string;
}

export class tvProgrammeCredits {
  private directorField: Array<string>;
  private actorField: Array<string>;
  private composerField: string;
  public keyword: Array<keyword>;
  public editor: Array<string>;
  public guest: Array<string>;
  public commentator: Array<string>;
  public producer: Array<string>;
  public adapter: Array<string>;
  public writer: Array<string>;
  public director: Array<string>;
  public actor: Array<string>;
  public composer: string;
}

export class tvProgrammeDesc {
  public lang: string;
  public Value: string;
}

export class tvProgrammeTitle {
  public lang: string;
  public Value: string;
}

export class tvProgramme {
  public DefaultTitle: string;
  public Id: string;
  public title: Array<tvProgrammeTitle>;
  public desc: Array<tvProgrammeDesc>;
  public credits: tvProgrammeCredits;
  public date: string;
  public category: Array<tvProgrammeCategory>;
  public length: tvProgrammeLength;
  public icon: tvProgrammeIcon;
  public video: tvProgrammeVideo;
  public audio: tvProgrammeAudio;
  public previouslyshown: Object;
  public subtitles: tvProgrammeSubtitles;
  public tvProgrammeepisodeNum: Array<string>;
  public rating: tvProgrammeRating;
  public starrating: tvProgrammeStarrating;
  public review: tvProgrammeReview;
  public start: string;
  public stop: string;
  public showview: string;
  public channel: string;
}

export class tv {
  constructor() {
    this.programme = new Array<tvProgramme>();
    this.channel = new Array<tvChannel>();
  }
  public channel: Array<tvChannel>;
  public programme: Array<tvProgramme>;
}

/**
 * Tvg channel (EPG model)
 * @export
 * @interface tvChannel
 */
export interface tvChannel {
  displayname: Array<string>;
  icon?: tvChannelIcon;
  id: string;
}

/**
 * Tvg channel Icon
 *
 * @export
 * @interface tvChannelIcon
 */
export interface tvChannelIcon {
  src: string;
}
