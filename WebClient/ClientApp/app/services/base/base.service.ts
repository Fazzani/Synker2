import { Injectable, Inject, Optional } from '@angular/core';
import { HttpClient } from '@angular/common/http';

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import { map, catchError } from 'rxjs/operators';
import { ElasticResponse, SimpleQueryElastic } from "../../types/elasticQuery.type";
import * as variables from '../../variables';

@Injectable()
export class BaseService {

    constructor(protected http: HttpClient, protected BaseUrl: string) { }

    search<T>(query: SimpleQueryElastic): Observable<ElasticResponse<T>> {
        return this.http.post(variables.BASE_API_URL + `${this.BaseUrl}/_searchstring`, query).map(res => {
            return res;
        }).catch(this.handleError);
    }

    simpleSearch<T>(query: string, indexName: string): Observable<ElasticResponse<T>> {
        return this.http.post(variables.BASE_API_URL + `${this.BaseUrl}/_searchstring`, <SimpleQueryElastic>{ From: 0, Size: 30, IndexName: indexName, Query: query }).map(res => {
            return res;
        }).catch(this.handleError);
    }

    delete(id: string): Observable<number> {
        return this.http.delete(variables.BASE_API_URL + `${this.BaseUrl}/${id}`).map(res => {
            return res;
        }).catch(this.handleError);
    }

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

    /**
     * Is Json 
     * @param {string} str
     * @returns
     */
    protected IsJsonString(str: string): boolean {
        try {
            JSON.parse(str);
        } catch (e) {
            return false;
        }
        return true;
    }
}