import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { ElasticQuery, ElasticResponse } from "../../types/elasticQuery.type";
import { tvChannel } from "../../types/xmltv.type";
import { sitePackChannel } from '../../types/sitepackchannel.type';
// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import { map, catchError } from 'rxjs/operators';
import * as variables from '../../variables';

@Injectable()
export class XmltvService extends BaseService {

    constructor(protected http: HttpClient) {
        super(http);
    }

    /**
     * Get Sitepack channel
     * @param {string} id
     * @returns
     */
    getSitePackChannel(id: string): Observable<ElasticResponse<sitePackChannel>> {
        return this.http.get(variables.BASE_API_URL + 'xmltv/channels/' + id).map(this.parseData)
            .catch(this.handleError);
    }

    /**
     * Liste xmltv from sitepack
     * @param {ElasticQuery} query
     * @returns
     */
    listSitePack(query: ElasticQuery): Observable<ElasticResponse<sitePackChannel>> {
        return this.http.post(variables.BASE_API_URL + 'xmltv/channels/_search', query).map(res => {
            return res;
        }).catch(this.handleError);
    }

    /**
     * Lancer le webgrab des xmltv_id passés en params
     * @param {string []} xmltv_id
     * @returns
     */
    webgrab(xmltv_id:string []) {
        return this.http.post(variables.BASE_API_URL + 'xmltv/channels/webgrab', xmltv_id).map(res => {
            return res;
        }).catch(this.handleError);
    }
}