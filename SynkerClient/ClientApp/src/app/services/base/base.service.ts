
import { throwError as observableThrowError, Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ElasticResponse, SimpleQueryElastic } from '../../types/elasticQuery.type';
import { environment } from '../../../environments/environment';

@Injectable()
export class BaseService {

  protected get FullBaseUrl(): string {
    return `${environment.base_api_url}${this._baseUrl}`;
}

  protected _baseUrl = '';
  constructor(protected http: HttpClient) { }

  search<T>(query: SimpleQueryElastic): Observable<ElasticResponse<T>> {
    return this.http
      .post(`${this.FullBaseUrl}/_searchstring`, query).pipe(
        map(this.handleSuccess),
        catchError(this.handleError), );
  }

  simpleSearch<T>(query: string, indexName: string): Observable<ElasticResponse<T>> {
    return this.http
      .post(`${this.FullBaseUrl}/_searchstring`, <SimpleQueryElastic>{
        From: 0,
        Size: 30,
        IndexName: indexName,
        Query: query
      }).pipe(
        map(this.handleSuccess),
        catchError(this.handleError), );
  }

  delete(id: string): Observable<number> {
    return this.http
      .delete(`${this.FullBaseUrl}/${id}`).pipe(
        map(this.handleSuccess),
        catchError(this.handleError), );
  }

  handleSuccess = (response: Response | any) => {
    // the return here is any, as you dont know how do the POJO(s) generated by response.json() look like
    return response || {};
  }

  protected handleError(error: Response | any) {
    let errorMessage: string;

    errorMessage = error.message ? error.message : error.toString();

    // In real world application, call to log error to remote server
    // logError(error);

    // This returns another Observable for the observer to subscribe to
    return observableThrowError(errorMessage);
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
