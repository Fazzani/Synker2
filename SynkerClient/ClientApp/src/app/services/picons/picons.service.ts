import { Observable } from "rxjs";
import { catchError, map } from 'rxjs/operators';
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import { ElasticQuery, ElasticResponse, SimpleQueryElastic } from "../../types/elasticQuery.type";
import { picon } from "../../types/picon.type";
import { TvgMedia } from "../../types/media.type";
import { environment } from "../../../environments/environment";

@Injectable({
  providedIn: 'root',
})
export class PiconService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http);
    this._baseUrl = "picons";
  }

  search(query: SimpleQueryElastic): Observable<ElasticResponse<picon>> {
    return super.search<picon>(query);
  }

  get(id: string): Observable<ElasticResponse<picon>> {
    return this.http
      .get(`${environment.base_api_url}${this._baseUrl}/${id}`).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  list(field: string, filter: string): Observable<ElasticResponse<picon>> {
    let q = ElasticQuery.Match(field, filter);

    return this.http
      .post(`${environment.base_api_url}${this._baseUrl}/_search`, q).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  delete(id: string): Observable<number> {
    return this.http
      .delete(`${environment.base_api_url}${this._baseUrl}/${id}`).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  synk(): Observable<ElasticResponse<picon>> {
    return this.http
      .post(`${environment.base_api_url}${this._baseUrl}/synk`, null).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  match(model: TvgMedia[], distance: number = 90, shouldMatchChannelNumber: boolean = true): Observable<TvgMedia[]> {
    return this.http
      .post(`${environment.base_api_url}${this._baseUrl}/match?distance=${distance}&shouldMatchChannelNumber=${shouldMatchChannelNumber}`, model).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }
}
