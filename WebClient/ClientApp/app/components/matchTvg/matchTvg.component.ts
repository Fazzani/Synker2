import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from '@angular/material';
import { CommonService, Constants } from '../../services/common/common.service';
import { TvgMediaService } from '../../services/tvgmedia/tvgmedia.service';
import { MatchTvgPostModel, MatchTvgFormModel, MatchingTvgSiteTypeEnum } from '../../types/matchTvgPostModel';
import { TvgMedia } from '../../types/media.type';
import { sitePackChannel } from '../../types/sitepackchannel.type';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs';

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
    progress: number = 50;
    matchingTvgSiteTypes: typeof MatchingTvgSiteTypeEnum;
    compareFn: ((f1: any, f2: any) => boolean) | null = this.compareByValue;

    obs: BehaviorSubject<TvgMedia> = new BehaviorSubject<TvgMedia>(null);

    constructor(
        public dialogRef: MatDialogRef<MatchTvgDialog>, @Inject(MAT_DIALOG_DATA) public tup: [TvgMedia[], string[]],
        private tvgMediaService: TvgMediaService, private commonService: CommonService) {
        this.matchingTvgSiteTypes = MatchingTvgSiteTypeEnum;
        [this.medias, this.tvgSites] = tup;
    }

    ngOnInit(): void {
    }

    onNoClick(): void {
        this.dialogRef.close();
    }

    compareByValue(f1: any, f2: any) {
        return f1 != null && f2 != null && f1 == f2;
    }

    /**
    * Try match tvg with Sites defined in media
    */
    matchTvg(): void {

        let model: MatchTvgPostModel = <MatchTvgPostModel>{ minScore: this.matchTvgFormModel.minScore };
        let mediasToMatch = this.medias.filter((v, i) => v.selected || this.matchTvgFormModel.matchAll);

        if (!this.matchTvgFormModel.overrideTvg)
            mediasToMatch = mediasToMatch.filter(x => x.tvg == null || x.tvg.id == '');

        let index = 0;

        mediasToMatch.forEach((x, i) => {

            model.mediaName = x.displayName;

            if (this.matchTvgFormModel.matchCountry)
                model.country = x.lang;

            switch (this.matchTvgFormModel.matchingTvgSiteType) {
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
                }
                this.obs.next(x);
                if (i == (mediasToMatch.length - 1))
                    this.obs.complete();
            });
        });

        this.obs.subscribe(x => {
            console.log(x);
        }, err => { }, () => this.dialogRef.close(this.medias));
    }

    ngOnDestroy() {
    }
}
