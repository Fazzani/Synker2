import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material";
import { Inject, Component, OnInit, OnDestroy } from "@angular/core";
import { TvgMedia } from "../../../types/media.type";
import Clappr from 'clappr';
import PlaybackRatePlugin from 'clappr-playback-rate-plugin';
import ClapprStats from 'clappr-stats';
//import ClapprNerdStats from 'clappr-nerd-stats';
import FLVJSPlayback from 'clappr-flvjs-playback';
import ChromecastPlugin from 'clappr-chromecast-plugin';
import LevelSelector from 'level-selector';
import { environment } from "../../../../environments/environment";
import { MediaServerService } from "../../../services/mediaServer/mediaserver.service";
//import ClapprPIPPlugin from 'clappr-pip-plugin';

@Component({
  selector: "media-watch-dialog",
  templateUrl: "./media.watch.dialog.html"
})
export class MediaWatchDialog implements OnInit, OnDestroy {
  player: any;
  live_url: string = environment.base_proxy_url + this.data.url.replace('.ts', '.m3u8');

  constructor(public dialogRef: MatDialogRef<MediaWatchDialog>, private mediaServerService: MediaServerService, @Inject(MAT_DIALOG_DATA) private data: TvgMedia) { }

  ngOnInit(): void {
    if (this.player) {
      this.ngOnDestroy();
    }

    var ErrorPlugin = Clappr.ContainerPlugin.extend({
      name: 'error_plugin',
      background: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAoAAAAFoBAMAAAA1HFdiAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAG1BMVEX5+fn//wAA//8A/wD/AP//AAAAAP8XFxf///8H5gWfAAAAAWJLR0QIht6VegAAAAd0SU1FB98IBRIsAXmGk48AAAI5SURBVHja7dJBDYBADADBs4AFLGABC1iohbOPhv1BMvu+NLlp10odqTN1pe7Uk5pQ8wMIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECBAgAABAgQIECDA/wKWxzM71T7ZZrfltNnppgACBAgQIECAAAECBAgQIECAAAECBAgQIECAAAECBAgQIECAAAECBAgQIECAAAECBAgQIECAAL8B+ALjSfYzPnmdzgAAACV0RVh0ZGF0ZTpjcmVhdGUAMjAxNS0wOC0wNVQxODo0NDowMSswMTowMCL95a4AAAAldEVYdGRhdGU6bW9kaWZ5ADIwMTUtMDgtMDVUMTg6NDQ6MDErMDE6MDBToF0SAAAAAElFTkSuQmCC',
      bindEvents: function () { this.listenTo(this.container, Clappr.Events.CONTAINER_ERROR, this.onError) },
      hide: function () { this._err && this._err.remove() },
      show: function () {
        var $ = Clappr.$
        this.hide();
        var txt = (this.options.errorPlugin && this.options.errorPlugin.text) ? this.options.errorPlugin.text : 'A fatal error occured.';
        this._err = $('<div>')
          .css({
            'position': 'absolute',
            'z-index': '999',
            'width': '100%',
            'height': '100%',
            'background-image': 'url(' + this.background + ')',
            'background-size': 'contain',
            'background-repeat': 'no-repeat',
            'padding-top': '15%',
            'text-align': 'center',
            'font-weight': 'bold',
            'text-shadow': '1px 1px #fff',
          })
          .append($('<h2>')
            .text(txt)
            .css({
              'font-size': '200%',
            }))
          .append($('<p>').html('Retrying in <span class="retry-counter" id="retry-counter">10</span> seconds ...')
            .css({
              'font-size': '120%',
              'margin': '15px',
            }));
        this.container && this.container.$el.prepend(this._err);
      },
      onError: function (e) {
        if (!this.container) return;
        this.show();
        this.container.getPlugin('click_to_pause').disable();
        var tid, t = 10, retry = function () {
          clearTimeout(tid);
          if (t === 0) {
            this.container.getPlugin('click_to_pause').enable();
            if (this.options.errorPlugin && this.options.errorPlugin.onRetry) {
              this.options.errorPlugin.onRetry(e);
              return;
            } else {
              this.container.stop();
              this.container.play();
              return;
            }
          }
          (<HTMLInputElement>document.getElementById("retry-counter")).innerText = t.toString();
          t--;
          tid = setTimeout(retry, 1000);
        }.bind(this);
        retry();
      }
    });

    var mss = this.mediaServerService;
    this.mediaServerService.live(this.data.url).subscribe(response => {
      console.log(`playing url: ${this.data.url}, displayName: ${this.data.displayName}, live: ${response.flvOutput}`);

      let player = this.player = new Clappr.Player({
        source: response.flvOutput,
        autoPlay: true,
        mimeType: 'video/flv',
        flushLiveURLCache: true,
        plugins: [PlaybackRatePlugin, ClapprStats, ChromecastPlugin, LevelSelector, ErrorPlugin, FLVJSPlayback],
        parentId: '#player',
        height: 700,
        width: 750,
        events: {
          onReady: function () { }, //Fired when the player is ready on startup
          onResize: function () { },//Fired when player resizes
          onPlay: function () { player.stopped = false; },//Fired when player starts to play
          onPause: function () { },//Fired when player pauses
          onStop: function () {
            if (!player.stopped) {
              mss.stop(response.streamId).subscribe(r => {
                player.stopped = true;
              });
            }
          },//Fired when player stops
          onEnded: function () { },//Fired when player ends the video
          onSeek: function () { },//Fired when player seeks the video
          onError: function () { },//Fired when player receives an error
          onTimeUpdate: function () { },//Fired when the time is updated on player
          onVolumeUpdate: function () { },//Fired when player updates its volume
        },
        errorPlugin: {
          // text: 'My custom error message.',
          onRetry: function (e) {
            // simulate successful recovery
            // or decide here what to do between each retry
            player.configure({
              source: response.flvOutput,
              autoPlay: true,
            });
          }
        },
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
        }, playback: {
          flvjsConfig: {
            isLive: true
          }
        }
      });

    });
  }

  onNoClick(): void {
    this.dialogRef.close();
  }

  ngOnDestroy(): void {
    console.log('ngOnDestroy');

    if (this.player !== undefined) {
      if (this.player.isPlaying()) {
        this.player.stop();
      }
      this.player.destroy();
    }
  }

}
