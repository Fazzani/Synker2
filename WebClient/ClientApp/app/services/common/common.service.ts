import { Injectable } from '@angular/core';

@Injectable()
export class CommonService {


    /**
     * @constructor
     */
    constructor() { }

    /**
     * Json to object Transformer
     * @param {string} json
     * @returns T
     */
    public JsonToObject<T>(json: string) {
        let res: T | null = null;
        try {
            res = <T>JSON.parse(json);
        } catch (e) {
            return res
        }
        return res;
    }
}


/**
 * Les contances de l'application
 */
export class Constants {
    //LocalStorage mediaQuery key
    static LS_MediaQueryKey: string = "mediaQuery";
    //LocalStorage epgQuery key
    static LS_EpgQueryKey: string = "epgQuery";
    static LS_MediaRefQueryKey: string = "mediaRefQuery";
}