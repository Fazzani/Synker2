import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppModuleShared } from './app.module.shared';
import { AppComponent } from './components/app/app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
//import { BASE_URL, BASE_API_URL} from './variables';

@NgModule({
    bootstrap: [AppComponent],
    imports: [
        BrowserModule,
        AppModuleShared,
        BrowserAnimationsModule
    ]
    //,
    //providers: [
    //    { provide: BASE_URL, useFactory: getBaseUrl },
    //    { provide: BASE_API_URL, useFactory: getBaseApiUrl }
    //]
})
export class AppModule {
}

//export function getBaseUrl() {
//    return document.getElementsByTagName('base')[0].href;
//}
//export function getBaseApiUrl() {
//    debugger;
//    return document.getElementsByTagName('meta')[0].getAttribute["data-api-url"];
//}