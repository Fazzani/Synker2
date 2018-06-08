"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/// <reference path="../../../../node_modules/@types/jasmine/index.d.ts" />
var testing_1 = require("@angular/core/testing");
var common_service_1 = require("./common.service");
var service;
var fixture;
describe('1st tests', function () {
    beforeEach(testing_1.async(function () {
        service = new common_service_1.CommonService(null);
    }));
    it('Json to object null', function () { return expect(service.JsonToObject("")).toBe(null); });
    it('Json to object => name == heni', function () { return expect(service.JsonToObject('{"name":"heni", "heni":2}').name).toBe("heni"); });
});
//# sourceMappingURL=common.service.spec.js.map