"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var MediaServerOptions = /** @class */ (function () {
    function MediaServerOptions() {
    }
    MediaServerOptions.Default = {
        Rtmp: 1935,
        Host: 'servermedia.synker.ovh',
        IsSecure: false,
        Auth: undefined,
        Port: 80
    };
    return MediaServerOptions;
}());
exports.MediaServerOptions = MediaServerOptions;
var BasicAuthOptions = /** @class */ (function () {
    function BasicAuthOptions() {
    }
    return BasicAuthOptions;
}());
exports.BasicAuthOptions = BasicAuthOptions;
var AuthOptions = /** @class */ (function () {
    function AuthOptions() {
    }
    return AuthOptions;
}());
exports.AuthOptions = AuthOptions;
var MediaServerLiveResponse = /** @class */ (function () {
    function MediaServerLiveResponse() {
    }
    return MediaServerLiveResponse;
}());
exports.MediaServerLiveResponse = MediaServerLiveResponse;
//# sourceMappingURL=mediaServerConfig.type.js.map