import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material";
import { Inject, Component, OnInit, OnDestroy } from "@angular/core";
import { TvgMedia } from "../../../types/media.type";
import Clappr from 'clappr';
import PlaybackRatePlugin from 'clappr-playback-rate-plugin';
import ClapprStats from 'clappr-stats';
//import ClapprNerdStats from 'clappr-nerd-stats';
import ChromecastPlugin from 'clappr-chromecast-plugin';
import LevelSelector from 'level-selector';
import { environment } from "../../../../environments/environment";
//import ClapprPIPPlugin from 'clappr-pip-plugin';

@Component({
  selector: "media-watch-dialog",
  templateUrl: "./media.watch.dialog.html"
})
export class MediaWatchDialog implements OnInit, OnDestroy {
  player: any;
  live_url: string = environment.base_proxy_url + this.data.url.replace('.ts', '.m3u8');

  constructor(public dialogRef: MatDialogRef<MediaWatchDialog>, @Inject(MAT_DIALOG_DATA) private data: TvgMedia) { }

  ngOnInit(): void {
    if (this.player) {
      this.ngOnDestroy();
    }

    console.log(`playing url: ${this.data.url}, displayName: ${this.data.displayName}, live: ${encodeURI(this.data.url.replace('.ts', '.m3u8').replace(/^https?:\/\//i, '//'))}`);

    this.player = new Clappr.Player({
      source: encodeURI(this.data.url.replace('.ts', '.m3u8')),
      autoPlay: true,
      flushLiveURLCache: true,
      plugins: [PlaybackRatePlugin, ClapprStats, ChromecastPlugin, LevelSelector],
      parentId: '#player',
      height: 700,
      width: 750,
      //hlsjsConfig: {
      //  enableWorker: true
      //},
      clapprNerdStats: {
        // Optional: provide multiple combination of keyboard shortcuts to show/hide the statistics.
        // For reference, visit: https://github.com/ccampbell/mousetrap.
        // Default: ['command+shift+s', 'ctrl+shift+s']
        shortcut: ['command+shift+s', 'ctrl+shift+s'],

        // Optional: position of the icon to show/hide the statistics.
        // Values: 'top-right', 'top-left', 'bottom-right', 'bottom-left', 'none'
        // Default: 'top-right'
        iconPosition: 'top-right'
      },
      levelSelectorConfig: {
        title: 'Quality',
        labels: {
          2: 'High', // 500kbps
          1: 'Med', // 240kbps
          0: 'Low', // 120kbps
        },
        labelCallback: function (playbackLevel, customLabel) {
          return customLabel + playbackLevel.level.height + 'p'; // High 720p
        },
        playbackRateConfig: {
          defaultValue: '1.0',
          options: [
            { value: '0.5', label: '0.5x' },
            { value: '1.0', label: '1x' },
            { value: '2.0', label: '2x' },
          ]
        },
      },
      chromecast: {
        appId: '6BDD3F23',
        contentType: 'video/mp4',
        media: {
          type: ChromecastPlugin.Movie,
          title: this.data.displayName,
          subtitle: this.data.name
        }
      }
    });
  }

  onNoClick(): void {
    this.player.destroy();
    this.dialogRef.close();
  }

  ngOnDestroy(): void {
    this.player.destroy();
  }

}
