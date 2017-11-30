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

    BuildElaticQuery(tabQeury: any[]): Object {

        if (tabQeury != undefined && tabQeury.length > 0) {
            if (tabQeury.length == 1)
                return tabQeury[0];

            let query: any;
            let tab: any[] = [];
            tabQeury.forEach(v => {
                tab.push(v);
            });

            query = {
                bool: { must: tab }
            };
            return query;
        }
        return null;
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