import { OnInit, OnDestroy, Component, ViewChild, Inject } from "@angular/core";
import { MediaType, TvgMedia, TvgSource, Tvg } from "../../../types/media.type";
import { sitePackChannel } from "../../../types/sitepackchannel.type";
import { Subject } from "rxjs";
import { MatAutocompleteTrigger, MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { SitePackService } from "../../../services/sitepack/sitepack.service";
import { CommonService } from "../../../services/common/common.service";
import { Observable } from "rxjs/Observable";

@Component({
    selector: 'tvgmedia.list.dialog',
    templateUrl: './tvgmedia.list.dialog.html'
})
export class PlaylistBulkUpdate implements OnInit, OnDestroy {
    mediaTypes: typeof MediaType;
    sitePacks: sitePackChannel[];
    cultures: string[];
    selectedLang: string;
    keyUpSitePack = new Subject<any>();
    filterChannelName: string;
    selectedMediaType: MediaType;
    enabled: boolean = true;
    data: TvgMedia[];

    groupsfiltred: string[];
    groups: string[];
    group: string;
    searchGroups$ = new Subject<KeyboardEvent>();
    @ViewChild(MatAutocompleteTrigger) autoTrigger: MatAutocompleteTrigger;

    constructor(private sitePackService: SitePackService, private commonService: CommonService,
        public dialogRef: MatDialogRef<PlaylistBulkUpdate>,
        @Inject(MAT_DIALOG_DATA) public tup: [TvgMedia[], Observable<string[]>]) {

        this.data = tup[0];
        tup[1].subscribe(x => this.groups = x);

        this.mediaTypes = MediaType;

        // AutoComplete sitepacks
        const subscription = this.keyUpSitePack
            .map(event => '*' + event.target.value + '*')
            .debounceTime(500)
            .distinctUntilChanged()
            .subscribe(search => sitePackService.sitePacks(search).subscribe(res => this.sitePacks = res));

        //AutoComplete groups
        this.searchGroups$
            .filter(x => x.keyCode != 13)
            .debounceTime(500)
            .map(m => m.key)
            .distinctUntilChanged()
            .do(() => { this.groupsfiltred = [] })
            .map(m => this.groups.filter(f => f.toLowerCase().indexOf(this.group.toLowerCase()) >= 0))
            .distinct()
            .subscribe(x => {
                console.log(x);
                this.groupsfiltred = x;
            });
    }

    onNewGroup(group: string): void {
        this.autoTrigger.closePanel();
        this.groups.push(group);
        this.onChangeGroup(group);
    }

    ngOnInit(): void {

        if (this.data != undefined && this.data.length > 0) {
            this.selectedLang = this.data[0].lang;
            this.selectedMediaType = this.data[0].mediaType;
        }

        this.sitePackService.countries().subscribe(c => {
            this.cultures = c;
        });
    }

    onChangeLang(event): void {
        console.log("culture was changed : ", this.selectedLang);
        this.data.forEach(m => m.lang = this.selectedLang);
    }

    onChangeGroup(g): void {
        console.log("Group was changed : ", g);
        this.data.forEach(m => m.group = g);
    }

    onChangeEnabled(enabled: boolean): void {
        console.log("disabled was changed : ", enabled);
        this.data.forEach(m => m.enabled = enabled);
    }

    onChangeMediaType(mediaType: number): void {
        console.log("MediaType was changed : ", mediaType);
        this.data.forEach(m => m.mediaType = mediaType);
    }

    onChangeTvgSourceSite(sitePack: sitePackChannel): void {
        console.log("tvgSourceSite was changed : ", sitePack);
        this.data.forEach(m => {

            if (m.tvg == null)
                m.tvg = <Tvg>{};
            if (m.tvg.tvgSource == null)
                m.tvg.tvgSource = <TvgSource>{};

            m.tvg.tvgSource.site = sitePack.site;
            m.tvg.tvgSource.country = sitePack.country;
        });
    }

    save(): void {
        this.dialogRef.close();
    }
    onNoClick(): void {
        this.dialogRef.close();
    }

    applyFixChannelName(replace: string): void {
        this.data
            .filter(x => new RegExp(this.filterChannelName).test(x.displayName))
            .forEach(x => {
                x.displayName = x.displayName.replace(new RegExp(this.filterChannelName), replace);
            });
    }

    ngOnDestroy(): void {
    }
}