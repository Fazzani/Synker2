import { ErrorHandler, Injectable, Injector } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CommonService } from '../services/common/common.service';

@Injectable()
export class ErrorsHandler implements ErrorHandler {
  constructor(
    private injector: Injector,
  ) { }

  handleError(error: any) {
    const commonService = this.injector.get(CommonService);
    //const router = this.injector.get(Router);
    error = error.rejection || error;
    if (error instanceof HttpErrorResponse) {
      // Server error happened      
      if (!navigator.onLine) {
        // No Internet connection
        return commonService.warn('Warning', 'No Internet Connection');
      }

      commonService.error(`${error.status}`, `${error.message}`);
    } else {
      //// Client Error Happend
      //// Send the error to the server and then
      //// redirect the user to the page with all the info
      //errorsService
      //  .log(error)
      //  .subscribe(errorWithContextInfo => {
      //    router.navigate(['/error'], { queryParams: errorWithContextInfo });
      //  });
    }

    // Log the error anyway
    console.error('It happens: ', error);
  }
}
