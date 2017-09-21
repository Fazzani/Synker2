import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';

@Injectable()
export class BaseService {
    static URL_API_BASE: string = 'http://localhost:56800/api/v1/';

    constructor(protected http: HttpClient) { }

    protected handleError(error: Response | any) {
        let errorMessage: string;

        errorMessage = error.message ? error.message : error.toString();

        // In real world application, call to log error to remote server
        // logError(error);

        // This returns another Observable for the observer to subscribe to
        return Observable.throw(errorMessage);
    }

    // This method parses the data to JSON
    protected parseData(res: Response) {
        return res.json() || [];
    }

}