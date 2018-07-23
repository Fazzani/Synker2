import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material";
import { Inject, Component, OnInit, OnDestroy } from "@angular/core";
import { TvgMedia } from "../../../types/media.type";
import * as Clappr from '../../../../../node_modules/clappr/dist/clappr.min.js';

@Component({
  selector: "media-watch-dialog",
  templateUrl: "./media.watch.dialog.html"
})
export class MediaWatchDialog implements OnInit, OnDestroy {
  player: any;
  constructor(public dialogRef: MatDialogRef<MediaWatchDialog>, @Inject(MAT_DIALOG_DATA) public data: TvgMedia) { }

  ngOnInit(): void {
    this.player = new Clappr.Player({
      source: this.data.url,
      autoPlay: true,
      plugins: [ChromecastPlugin, LevelSelector, ClapprNerdStats, ClapprStats, ClapprPIPPlugin],
      parentId: '#player',
      height: 607.5,
      width: 1080,
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
        }
      },
      chromecast: {
        appId: '9DFB77C0',
        contentType: 'video/mp4',
        media: {
          type: ChromecastPlugin.Movie,
          title: 'Awesome Hot Air Balloon Slackline',
          subtitle: 'You won\'t get much closer to Skylining than this!'
        }
      }
    });
  }

  onNoClick(): void {
    this.dialogRef.close();
  }

  ngOnDestroy(): void {
    this.player = null;
  }

}
