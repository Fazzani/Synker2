import { AboutApplication } from "../../types/aboutApplication.type";
import { Injectable } from "@angular/core";
import { Constants } from "../common/common.service";

@Injectable()
export class InitAppService {
  about: AboutApplication;
  /**
   * @constructor
   */
  constructor() {}

  /**
   * About Application
   */
  getAboutApplication(): Promise<any> {
    return new Promise((resolve, reject) => {
      const xhr = new XMLHttpRequest();
      xhr.open("GET", "/about");

      xhr.addEventListener("readystatechange", () => {
        if (xhr.readyState === XMLHttpRequest.DONE && xhr.status === 200) {
          this.about = JSON.parse(xhr.responseText);
          localStorage.setItem(Constants.LS_ABOUT_APP_KEY, xhr.responseText);
          console.log(`About: ${this.about}`);
          resolve(this.about);
        } else if (xhr.readyState === XMLHttpRequest.DONE) {
          reject();
        }
      });

      xhr.send(null);
    });
  }
}
