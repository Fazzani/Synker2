import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable()
export class CommonService {

    public loaderStatus: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(true);

    /**
     * @constructor
     */
    constructor() { }

    displayLoader(value: boolean) {
        this.loaderStatus.next(value);
    }

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

    stringEnumToKeyValue(stringEnum) {
        const keyValue = [];
        const keys = Object.keys(stringEnum).filter((value, index) => {
            return !(index % 2);
        });

        for (const k of keys) {
            keyValue.push({ key: k, value: stringEnum[k] });
        }

        return keyValue;
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
    static LS_SiteQueryKey: string = "SitePackQuery";
    static MediaPageListKey: string = "mediaPageList";
}