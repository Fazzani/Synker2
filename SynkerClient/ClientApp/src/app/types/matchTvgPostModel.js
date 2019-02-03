"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var MatchTvgPostModel = /** @class */ (function () {
    function MatchTvgPostModel() {
        this.minScore = 0.5;
    }
    return MatchTvgPostModel;
}());
exports.MatchTvgPostModel = MatchTvgPostModel;
var MatchTvgFormModel = /** @class */ (function () {
    function MatchTvgFormModel() {
        this.minScore = 0.5;
        this.overrideTvg = true;
        this.matchAll = false;
        this.matchingTvgSiteType = MatchingTvgSiteTypeEnum.TvgSiteInMedia;
    }
    return MatchTvgFormModel;
}());
exports.MatchTvgFormModel = MatchTvgFormModel;
var MatchingTvgSiteTypeEnum;
(function (MatchingTvgSiteTypeEnum) {
    MatchingTvgSiteTypeEnum[MatchingTvgSiteTypeEnum["TvgSiteInMedia"] = 0] = "TvgSiteInMedia";
    MatchingTvgSiteTypeEnum[MatchingTvgSiteTypeEnum["TvgSitePlaylist"] = 1] = "TvgSitePlaylist";
    MatchingTvgSiteTypeEnum[MatchingTvgSiteTypeEnum["All"] = 2] = "All";
})(MatchingTvgSiteTypeEnum = exports.MatchingTvgSiteTypeEnum || (exports.MatchingTvgSiteTypeEnum = {}));
//# sourceMappingURL=matchTvgPostModel.js.map