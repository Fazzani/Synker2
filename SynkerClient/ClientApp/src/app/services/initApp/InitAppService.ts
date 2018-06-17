import { AboutApplication } from "../../types/aboutApplication.type";
import { Injectable } from "@angular/core";
import { Constants } from "../common/common.service";
import { environment } from "../../../environments/environment";
const aboutDefault: AboutApplication = <AboutApplication>{
  ApplicationName: "Synker",
  Author: "Synker",
  LastUpdate: new Date().toLocaleTimeString(),
  License: "MIT",
  Version: "1.0.0-beta"
};
@Injectable({
  providedIn: "root"
})
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
          if (!environment.production) {
            reject();
          } else {
            localStorage.setItem(Constants.LS_ABOUT_APP_KEY, JSON.stringify(aboutDefault));
            this.about = aboutDefault;
            resolve(this.about);
          }
        }
      });

      xhr.send(null);
    });
  }
}
