import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { BehaviorSubject } from 'rxjs';
import { MatchTvgPostModel, MatchTvgFormModel, MatchingTvgSiteTypeEnum } from '../../../types/matchTvgPostModel';
import { TvgMediaService } from '../../../services/tvgmedia/tvgmedia.service';
import { TvgMedia } from '../../../types/media.type';
import { sitePackChannel } from '../../../types/sitepackchannel.type';

/**
 *  url tests
 * http://www.m3uliste.pw/
 */
@Component({
  selector: 'matchtvg-dialog',
  templateUrl: './matchtvg.dialog.html'
})
export class MatchTvgDialog implements OnInit, OnDestroy {
  tvgSites: string[];
  medias: TvgMedia[];
  matchTvgFormModel: MatchTvgFormModel = new MatchTvgFormModel();
  progress = 0;
  matchingTvgSiteTypes: typeof MatchingTvgSiteTypeEnum;
  compareFn: ((f1: any, f2: any) => boolean) | null = this.compareByValue;

  obs: BehaviorSubject<TvgMedia> = new BehaviorSubject<TvgMedia>(null);

  constructor(
    public dialogRef: MatDialogRef<MatchTvgDialog>,
    @Inject(MAT_DIALOG_DATA) public tup: [TvgMedia[], string[]],
    private tvgMediaService: TvgMediaService
  ) {
    this.matchingTvgSiteTypes = MatchingTvgSiteTypeEnum;
    [this.medias, this.tvgSites] = tup;
  }

  ngOnInit(): void { }

  onNoClick(): void {
    this.dialogRef.close();
  }

  compareByValue(f1: any, f2: any) {
    return f1 != null && f2 != null && f1 === f2;
  }

  /**
   * Try match tvg with Sites defined in media
   */
  matchTvg(): void {
    const model: MatchTvgPostModel = <MatchTvgPostModel>{
      minScore: this.matchTvgFormModel.minScore
    };
    let mediasToMatch = this.medias.filter((v, i) => v.selected || this.matchTvgFormModel.matchAll);
    if (!this.matchTvgFormModel.overrideTvg) { mediasToMatch = mediasToMatch.filter(x => x.tvg == null || x.tvg.id === ''); }

    let index = 0;

    mediasToMatch.forEach((x, i) => {
      model.mediaName = x.displayName;

      if (this.matchTvgFormModel.matchCountry) { model.country = x.lang; }

      switch (parseInt(this.matchTvgFormModel.matchingTvgSiteType, 10)) {
        case MatchingTvgSiteTypeEnum.TvgSiteInMedia:
          model.tvgSites = [x.tvg.tvgSource.site];
          break;
        case MatchingTvgSiteTypeEnum.TvgSitePlaylist:
          model.tvgSites = this.tvgSites;
          break;
      }

      this.tvgMediaService.matchTvg(model).subscribe((res: sitePackChannel) => {
        this.progress = (++index / mediasToMatch.length) * 100;
        console.log(this.progress);
        if (res != null) {
          console.log(`media was ${x} matched with sitepack ${res}`);
          x.tvg.id = res.xmltv_id;
          x.tvg.name = res.channel_name;
          x.tvg.tvgSource.site = res.site;
          x.tvg.tvgSource.code = res.site_id;
          x.tvg.tvgSource.country = res.country;
          x.tvg.tvgIdentify = res.site_id;
        }
        this.obs.next(x);
        if (i === mediasToMatch.length - 1) { this.obs.complete(); }
      });
    });

    this.obs.subscribe(
      x => {
        console.log(x);
      },
      err => { },
      () => this.dialogRef.close(this.medias)
    );
  }

  ngOnDestroy() { }
}
