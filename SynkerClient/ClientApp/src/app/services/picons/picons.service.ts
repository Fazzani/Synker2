import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import {
  ElasticQuery,
  ElasticResponse,
  ElasticAggregations,
  SimpleQueryElastic
} from "../../types/elasticQuery.type";

// All the RxJS stuff we need
import { Observable } from "rxjs/Rx";
import { map } from "rxjs/operators";
import * as variables from "../../variables";
import { picon } from "../../types/picon.type";
import { TvgMedia } from "../../types/media.type";
import { environment } from "../../../environments/environment";

@Injectable()
export class PiconService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http, "picons");
  }

  search(query: SimpleQueryElastic): Observable<ElasticResponse<picon>> {
    return super.search<picon>(query);
  }

  get(id: string): Observable<ElasticResponse<picon>> {
    return this.http
      .get(`${environment.base_api_url}${this.BaseUrl}/${id}`)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }

  list(field: string, filter: string): Observable<ElasticResponse<picon>> {
    let q = ElasticQuery.Match(field, filter);

    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/_search`, q)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }

  delete(id: string): Observable<number> {
    return this.http
      .delete(`${environment.base_api_url}${this.BaseUrl}/${id}`)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }

  synk(): Observable<ElasticResponse<picon>> {
    return this.http
      .post(`${environment.base_api_url}${this.BaseUrl}/synk`, null)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }

  match(
    model: TvgMedia[],
    distance: number = 90,
    shouldMatchChannelNumber: boolean = true
  ): Observable<TvgMedia[]> {
    return this.http
      .post(
        `${environment.base_api_url}${
          this.BaseUrl
        }/match?distance=${distance}&shouldMatchChannelNumber=${shouldMatchChannelNumber}`,
        model
      )
      .map(this.handleSuccess)
      .catch(this.handleError);
  }
}
