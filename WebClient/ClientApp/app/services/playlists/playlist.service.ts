﻿import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import { map, catchError } from 'rxjs/operators';
import { RequestOptions } from "@angular/http/http";
import { HttpHeaders } from "@angular/common/http";
import * as variables from "../../variables";
import { Playlist, PlaylistModel } from '../../types/playlist.type';
import { QueryListBaseModel, PagedResult } from '../../types/common.type';

@Injectable()
export class PlaylistService extends BaseService {

    constructor(protected http: HttpClient) { super(http); }

    get(id: string): Observable<PlaylistModel> {
        return this.http.get(variables.BASE_API_URL + 'playlists/' + id).map(this.parseData)
            .catch(this.handleError);
    }

    list(query: QueryListBaseModel): Observable<PagedResult<PlaylistModel>> {
        return this.http.post(variables.BASE_API_URL + 'playlists/search', query).map(res => {
            return res;
        }).catch(this.handleError);
    }

    synk(): Observable<PlaylistModel> {
        return this.http.post(variables.BASE_API_URL + 'playlists/synk/', null).map(res => {
            return res;
        }).catch(this.handleError);
    }

    //export(fromType: string, toType: string, ): Observable<any> {
    //    return this.http.post(variables.BASE_API_URL + 'playlists/export/' + fromType + '/' + toType, null).map(res => {
    //        return res;
    //    }).catch(this.handleError);
    //}

    update(p: Playlist): Observable<Playlist> {
        return this.http.put(variables.BASE_API_URL + 'playlists', p).map(res => {
            return res;
        }).catch(this.handleError);
    }

    delete(id: string): Observable<Playlist> {
        return this.http.delete(variables.BASE_API_URL + 'playlists/' + id).map(res => {
            return res;
        }).catch(this.handleError);
    }
}