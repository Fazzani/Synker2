import { Observable } from "rxjs";
import { catchError, map } from 'rxjs/operators';
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import { tvChannel } from "../../types/xmltv.type";
import { environment } from "../../../environments/environment";

@Injectable({
  providedIn: 'root',
})
export class EpgService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http);
    this._baseUrl = "epg";
  }

  get(id: string): Observable<ElasticResponse<tvChannel>> {
    return this.http
      .get(environment.base_api_url + `${this._baseUrl}/${id}`).pipe(
        map(this.handleSuccess));
  }

  list(query: ElasticQuery): Observable<ElasticResponse<tvChannel>> {
    return this.http
      .post(environment.base_api_url + "epg/_search/", query).pipe(
        map(this.handleSuccess));
  }
}
