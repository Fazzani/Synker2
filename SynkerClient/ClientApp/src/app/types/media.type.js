"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    }
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * Tvg media (channel)
 *
 * @export
 * @interface TvgMedia
 */
var TvgMedia = /** @class */ (function () {
    function TvgMedia() {
    }
    return TvgMedia;
}());
exports.TvgMedia = TvgMedia;
var MediaType;
(function (MediaType) {
    MediaType[MediaType["LiveTv"] = 0] = "LiveTv";
    MediaType[MediaType["Radio"] = 1] = "Radio";
    MediaType[MediaType["Video"] = 2] = "Video";
    MediaType[MediaType["Audio"] = 3] = "Audio";
    MediaType[MediaType["Other"] = 4] = "Other";
})(MediaType = exports.MediaType || (exports.MediaType = {}));
var Culture = /** @class */ (function () {
    function Culture() {
    }
    return Culture;
}());
exports.Culture = Culture;
var TvgSource = /** @class */ (function (_super) {
    __extends(TvgSource, _super);
    function TvgSource() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    return TvgSource;
}(Culture));
exports.TvgSource = TvgSource;
var MediaGroup = /** @class */ (function () {
    function MediaGroup() {
    }
    return MediaGroup;
}());
exports.MediaGroup = MediaGroup;
//# sourceMappingURL=media.type.js.map