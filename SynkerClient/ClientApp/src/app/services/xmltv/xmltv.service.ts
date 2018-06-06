import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import { tvChannel } from "../../types/xmltv.type";
import { sitePackChannel } from "../../types/sitepackchannel.type";
// All the RxJS stuff we need
import { Observable } from "rxjs/Rx";
import { map, catchError } from "rxjs/operators";
import * as variables from "../../variables";
import { environment } from "../../../environments/environment";

@Injectable()
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
      .get(environment.base_api_url + "xmltv/channels/" + id)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }

  /**
   * Liste xmltv from sitepack
   * @param {ElasticQuery} query
   * @returns
   */
  listSitePack(
    query: ElasticQuery
  ): Observable<ElasticResponse<sitePackChannel>> {
    return this.http
      .post(environment.base_api_url + "xmltv/channels/_search", query)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }

  /**
   * Lancer le webgrab des xmltv_id passés en params
   * @param {string []} xmltv_id
   * @returns
   */
  webgrab(xmltv_id: string[]) {
    return this.http
      .post(environment.base_api_url + "xmltv/channels/webgrab", xmltv_id)
      .map(this.handleSuccess)
      .catch(this.handleError);
  }
}
