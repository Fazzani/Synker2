"use strict";
var __extends = (this && this.__extends) || (function () {
  var extendStatics = Object.setPrototypeOf ||
    ({
        __proto__: []
      }
      instanceof Array && function (d, b) {
        d.__proto__ = b;
      }) ||
    function (d, b) {
      for (var p in b)
        if (b.hasOwnProperty(p)) d[p] = b[p];
    };
  return function (d, b) {
    extendStatics(d, b);

    function __() {
      this.constructor = d;
    }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
  };
})();
Object.defineProperty(exports, "__esModule", {
  value: true
});
var rxjs_1 = require("rxjs");
var WebSocketSubject_1 = require("rxjs/observable/dom/WebSocketSubject");
/// we inherit from the ordinary Subject
var RxWebsocketSubject = /** @class */ (function (_super) {
  __extends(RxWebsocketSubject, _super);

  function RxWebsocketSubject(url, reconnectInterval, /// pause between connections
    reconnectAttempts, /// number of connection attempts
    resultSelector, serializer) {
    if (reconnectInterval === void 0) {
      reconnectInterval = 5000;
    }
    if (reconnectAttempts === void 0) {
      reconnectAttempts = 10;
    }
    var _this = _super.call(this) || this;
    _this.url = url;
    _this.reconnectInterval = reconnectInterval;
    _this.reconnectAttempts = reconnectAttempts;
    _this.resultSelector = resultSelector;
    _this.serializer = serializer;
    /// by default, when a message is received from the server, we are trying to decode it as JSON
    /// we can override it in the constructor
    _this.defaultResultSelector = function (e) {
      return JSON.parse(e.data);
    };
    /// when sending a message, we encode it to JSON
    /// we can override it in the constructor
    _this.defaultSerializer = function (data) {
      return JSON.stringify(data);
    };
    /// connection status
    _this.connectionStatus = new rxjs_1.Observable(function (observer) {
        _this.connectionObserver = observer;
      })
      .share()
      .distinctUntilChanged();
    if (!resultSelector) {
      _this.resultSelector = _this.defaultResultSelector;
    }
    if (!_this.serializer) {
      _this.serializer = _this.defaultSerializer;
    }
    /// config for WebSocketSubject
    /// except the url, here is closeObserver and openObserver to update connection status
    _this.wsSubjectConfig = {
      url: url,
      closeObserver: {
        next: function (e) {
          _this.socket = null;
          _this.connectionObserver.next(false);
        }
      },
      openObserver: {
        next: function (e) {
          _this.connectionObserver.next(true);
        }
      }
    };
    /// we connect
    _this.connect();
    /// we follow the connection status and run the reconnect while losing the connection
    _this.connectionStatus.subscribe(function (isConnected) {
      if (!_this.reconnectionObservable && typeof (isConnected) == "boolean" && !isConnected) {
        _this.reconnect();
      }
    });
    return _this;
  }
  RxWebsocketSubject.prototype.connect = function () {
    var _this = this;
    this.socket = new WebSocketSubject_1.WebSocketSubject(this.wsSubjectConfig);
    this.socket.subscribe(function (m) {
      _this.next(m); /// when receiving a message, we just send it to our Subject
    }, function (error) {
      if (!_this.socket) {
        /// in case of an error with a loss of connection, we restore it
        _this.reconnect();
      }
    });
  };
  /// WebSocket Reconnect handling
  RxWebsocketSubject.prototype.reconnect = function () {
    var _this = this;
    this.reconnectionObservable = rxjs_1.Observable.interval(this.reconnectInterval)
      .takeWhile(function (v, index) {
        return index < _this.reconnectAttempts && !_this.socket;
      });
    this.reconnectionObservable.subscribe(function () {
      _this.connect();
    }, null, function () {
      /// if the reconnection attempts are failed, then we call complete of our Subject and status
      _this.reconnectionObservable = null;
      if (!_this.socket) {
        _this.complete();
        _this.connectionObserver.complete();
      }
    });
  };
  /// sending the message
  RxWebsocketSubject.prototype.send = function (data) {
    this.socket.next(this.serializer(data));
  };
  return RxWebsocketSubject;
}(rxjs_1.Subject));
exports.RxWebsocketSubject = RxWebsocketSubject;
//# sourceMappingURL=wsReconnectionSubject.type.js.map
