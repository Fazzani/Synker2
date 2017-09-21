/// <reference path="../../../../node_modules/@types/jasmine/index.d.ts" />
import { TestBed, async, ComponentFixture, ComponentFixtureAutoDetect } from '@angular/core/testing';
import { CommonService } from "./common.service";

let service: CommonService;
let fixture: ComponentFixture<CommonService>;

describe('1st tests', () => {
    beforeEach(async(() =>
    {
        service = new CommonService();
    }));

    it('Json to object null', () => expect(service.JsonToObject("")).toBe(null));
    it('Json to object => name == heni', () => expect(service.JsonToObject<any>('{"name":"heni", "heni":2}').name).toBe("heni"));
});