import { Observable } from "rxjs";
import { map, catchError } from 'rxjs/operators';
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import { sitePackChannel } from "../../types/sitepackchannel.type";
import { environment } from "../../../environments/environment";

@Injectable({
  providedIn: 'root',
})
export class XmltvService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http, "xmltv");
  }

  /**
   * Get Sitepack channel
   * @param {string} id
   * @returns
   */
  getSitePackChannel(id: string): Observable<ElasticResponse<sitePackChannel>> {
    return this.http
      .get(environment.base_api_url + "xmltv/channels/" + id).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  /**
   * Liste xmltv from sitepack
   * @param {ElasticQuery} query
   * @returns
   */
  listSitePack(query: ElasticQuery): Observable<ElasticResponse<sitePackChannel>> {
    return this.http
      .post(environment.base_api_url + "xmltv/channels/_search", query).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  /**
   * Lancer le webgrab des xmltv_id passés en params
   * @param {string []} xmltv_id
   * @returns
   */
  webgrab(xmltv_id: string[]) {
    return this.http
      .post(environment.base_api_url + "xmltv/channels/webgrab", xmltv_id).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }
}
