import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { Exception } from "../../types/common.type";
import { ToastrService } from "ngx-toastr";

@Injectable()
export class CommonService {
  /**
   * @constructor
   */
  constructor(private toastyService: ToastrService) {}
  public loaderStatus: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(true);
  public error: BehaviorSubject<Exception> = new BehaviorSubject<Exception>(null);

  info(title: string, message: string): void {
    this.toastyService.info(message, title);
  }

  success(title: string, message: string): void {
    this.toastyService.success(message, title);
  }

  displayError(title: string, message: string): void {
    this.error.next(<Exception>{ title: title, message: message });
  }

  displayLoader(value: boolean) {
    console.log("displayLoader => value", value);
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
      return res;
    }
    return res;
  }

  BuildElaticQuery(tabQeury: any[]): Object {
    if (tabQeury != undefined && tabQeury.length > 0) {
      if (tabQeury.length == 1) return tabQeury[0];

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
  static LS_ABOUT_APP_KEY: string = "LS_ABOUT_APP_KEY";
  static MediaPageListKey: string = "mediaPageList";
  static ThemeKey: string = "theme";
  static ThemesList = ["default-theme", "dark-theme", "light-theme"];
}
