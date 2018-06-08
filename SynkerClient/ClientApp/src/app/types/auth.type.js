"use strict";
Object.defineProperty(exports, "__esModule", {
  value: true
});
/**
 * User entity
 * @description User entity
 */
var User = /** @class */ (function () {
  function User() {
    this.roles = "Default";
  }
  User.GENDERS = [{
      value: 0,
      viewValue: "Mr"
    },
    {
      value: 1,
      viewValue: "Mrs"
    }
  ];
  return User;
}());
exports.User = User;
var AuthModel = /** @class */ (function () {
  function AuthModel() {}
  return AuthModel;
}());
exports.AuthModel = AuthModel;
var GrantType;
(function (GrantType) {
  GrantType[GrantType["password"] = 0] = "password";
  GrantType[GrantType["refreshToken"] = 1] = "refreshToken";
})(GrantType = exports.GrantType || (exports.GrantType = {}));
var ConnectionState = /** @class */ (function () {
  function ConnectionState() {}
  return ConnectionState;
}());
exports.ConnectionState = ConnectionState;
//# sourceMappingURL=auth.type.js.map
