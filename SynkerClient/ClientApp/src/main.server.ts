import 'zone.js/dist/zone-node';
import 'reflect-metadata';
import { renderModule, renderModuleFactory } from '@angular/platform-server';
import { APP_BASE_HREF } from '@angular/common';
import { enableProdMode } from '@angular/core';
import { provideModuleMap } from '@nguniversal/module-map-ngfactory-loader';
import { createServerRenderer } from 'aspnet-prerendering';
export { AppServerModule } from './app/app.server.module';

enableProdMode();

export default createServerRenderer(params => {
  const { AppServerModule, AppServerModuleNgFactory, LAZY_MODULE_MAP } = (module as any).exports;

  const options = {
    document: params.data.originalHtml,
    url: params.url,
    extraProviders: [
      provideModuleMap(LAZY_MODULE_MAP),
      { provide: APP_BASE_HREF, useValue: params.baseUrl },
      { provide: 'BASE_URL', useValue: params.origin + params.baseUrl },
      { provide: 'ABOUT', useValue: params.data.about },
    ]
  };

  const renderPromise = AppServerModuleNgFactory
    ? /* AoT */ renderModuleFactory(AppServerModuleNgFactory, options)
    : /* dev */ renderModule(AppServerModule, options);

  return renderPromise.then(html => ({ html }));
});

// import 'zone.js/dist/zone-node';
// import './polyfills.server';

// import { AppServerModule } from './app/app.server.module';
// import { enableProdMode } from '@angular/core';
// import { INITIAL_CONFIG } from '@angular/platform-server';
// // import { APP_BASE_HREF } from '@angular/common';
// import { createServerRenderer, RenderResult, BootFuncParams } from 'aspnet-prerendering';

// // import { ORIGIN_URL } from './app/shared/constants/baseurl.constants';
// // Grab the (Node) server-specific NgModule
// // Temporary * the engine will be on npm soon (`@universal/ng-aspnetcore-engine`)
// import { ngAspnetCoreEngine, IEngineOptions, createTransferScript } from '@nguniversal/aspnetcore-engine';
// enableProdMode();

// export default createServerRenderer((params: BootFuncParams) => {

//   // Platform-server provider configuration
//   const setupOptions: IEngineOptions = {
//     appSelector: '<app></app>',
//     ngModule: AppServerModule,
//     request: params,
//     providers: [
//       // Optional - Any other Server providers you want to pass (remember you'll have to provide them for the Browser as well)
//     ]
//   };

//   return ngAspnetCoreEngine(setupOptions).then(response => {
//     // Apply your transferData to response.globals
//     response.globals.transferData = createTransferScript({
//       someData: 'Transfer this to the client on the window.TRANSFER_CACHE {} object',
//       about: params.data.about
//     });

//     return ({
//       html: response.html,
//       globals: response.globals
//     });
//   });
// });
