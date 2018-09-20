"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var SynkConfig = /** @class */ (function () {
    function SynkConfig() {
    }
    return SynkConfig;
}());
exports.SynkConfig = SynkConfig;
var SynkGroupEnum;
(function (SynkGroupEnum) {
    SynkGroupEnum[SynkGroupEnum["none"] = 0] = "none";
    SynkGroupEnum[SynkGroupEnum["byCountry"] = 1] = "byCountry";
    SynkGroupEnum[SynkGroupEnum["byLanguage"] = 2] = "byLanguage";
    SynkGroupEnum[SynkGroupEnum["custom"] = 3] = "custom";
})(SynkGroupEnum = exports.SynkGroupEnum || (exports.SynkGroupEnum = {}));
var PlaylistStatus;
(function (PlaylistStatus) {
    PlaylistStatus[PlaylistStatus["enabled"] = 0] = "enabled";
    PlaylistStatus[PlaylistStatus["disabled"] = 1] = "disabled";
})(PlaylistStatus = exports.PlaylistStatus || (exports.PlaylistStatus = {}));
var Providers;
(function (Providers) {
    Providers[Providers["m3u"] = 0] = "m3u";
    Providers[Providers["tvlist"] = 1] = "tvlist";
    Providers[Providers["xtream"] = 2] = "xtream";
})(Providers = exports.Providers || (exports.Providers = {}));
var NotificationTypeEnum;
(function (NotificationTypeEnum) {
    NotificationTypeEnum[NotificationTypeEnum["pushBrowser"] = 1] = "pushBrowser";
    NotificationTypeEnum[NotificationTypeEnum["pushMobile"] = 2] = "pushMobile";
    NotificationTypeEnum[NotificationTypeEnum["email"] = 4] = "email";
    NotificationTypeEnum[NotificationTypeEnum["sms"] = 8] = "sms";
})(NotificationTypeEnum = exports.NotificationTypeEnum || (exports.NotificationTypeEnum = {}));
var PlaylistModel = /** @class */ (function () {
    function PlaylistModel() {
    }
    PlaylistModel.PROVIDERS = Object.keys(Providers).slice(Object.keys(Providers).length / 2);
    PlaylistModel.SYNKGROUP = Object.keys(SynkGroupEnum).slice(Object.keys(SynkGroupEnum).length / 2);
    return PlaylistModel;
}());
exports.PlaylistModel = PlaylistModel;
var PlaylistModelLive = /** @class */ (function () {
    function PlaylistModelLive() {
    }
    PlaylistModelLive.PROVIDERS = Object.keys(Providers).slice(Object.keys(Providers).length / 2);
    PlaylistModelLive.SYNKGROUP = Object.keys(SynkGroupEnum).slice(Object.keys(SynkGroupEnum).length / 2);
    return PlaylistModelLive;
}());
exports.PlaylistModelLive = PlaylistModelLive;
var PlaylistPostModel = /** @class */ (function () {
    function PlaylistPostModel() {
    }
    return PlaylistPostModel;
}());
exports.PlaylistPostModel = PlaylistPostModel;
//# sourceMappingURL=playlist.type.js.map