import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';

// All the RxJS stuff we need
import { Observable } from 'rxjs/Observable';
import { map, catchError } from 'rxjs/operators';
import { RequestOptions } from "@angular/http/http";
import { HttpHeaders, HttpParams } from "@angular/common/http";
import * as variables from "../../variables";
import { PlaylistModel, PlaylistPostModel } from '../../types/playlist.type';
import { QueryListBaseModel, PagedResult } from '../../types/common.type';
import { TvgMedia } from '../../types/media.type';

@Injectable()
export class PlaylistService extends BaseService {

    constructor(protected http: HttpClient) { super(http, 'playlists'); }

    get(id: string, light: boolean): Observable<PlaylistModel> {

        let params = new HttpParams();
        params = params.set("light", light ? "true" : "false");

        return this.http.get(`${variables.BASE_API_URL}${this.BaseUrl}/${id}`, { params: params })
            .catch(this.handleError);
    }

    list(query: QueryListBaseModel): Observable<PagedResult<PlaylistModel>> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/search`, query).map(res => {
            return res;
        }).catch(this.handleError);
    }

    synk(model: PlaylistPostModel): Observable<PlaylistModel> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/synk`, model).map(res => {
            return res;
        }).catch(this.handleError);
    }

    executeHandlers(model: TvgMedia[]): Observable<TvgMedia[]> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/handlers`, model).map(res => {
            return res;
        }).catch(this.handleError);
    }

    match(id: string): Observable<PlaylistModel> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/match/$${id}`, null).map(res => {
            return res;
        }).catch(this.handleError);
    }

    matchtvg(id: string, onlyNotMatched: boolean = true): Observable<PlaylistModel> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/matchtvg/${id}?onlyNotMatched=${onlyNotMatched}`, null).map(res => {
            return res;
        }).catch(this.handleError);
    }

    matchFiltredTvgSites(id: string, onlyNotMatched: boolean = true): Observable<PlaylistModel> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/matchfiltred/${id}?onlyNotMatched=${onlyNotMatched}`, null).map(res => {
            return res;
        }).catch(this.handleError);
    }

    /**
    * Match VOD Playlist
    * @param {string} id
    * @returns
    */
    matchVideosByPlaylist(id: string): Observable<PlaylistModel> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/matchvideos/${id}`, null).map(res => {
            return res;
        }).catch(this.handleError);
    }

    /**
     * Match videos VOD
     * @param {string} id
     * @returns
     */
    matchVideos(...medias: TvgMedia[]): Observable<TvgMedia[]> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/matchvideos`, medias).map(res => {
            return res;
        }).catch(this.handleError);
    }

    /**
     * Match VOD by name
     * @param {string} mediaName
     * @returns Poster Paht
     */
    matchVideo(mediaName: string): Observable<any> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/matchvideo/${mediaName}`, null).map(res => {
            return res;
        }).catch(this.handleError);
    }

    //export(fromType: string, toType: string, ): Observable<any> {
    //    return this.http.post(variables.BASE_API_URL + 'playlists/export/' + fromType + '/' + toType, null).map(res => {
    //        return res;
    //    }).catch(this.handleError);
    //}

    diff(model: PlaylistPostModel): Observable<PlaylistModel> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/diff`, model).map(res => {
            return res;
        }).catch(this.handleError);
    }

    addByUrl(p: PlaylistPostModel): Observable<PlaylistModel> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/create`, p,
            { headers: new HttpHeaders().set('Content-Type', 'application/json'), responseType: 'text' }).catch(this.handleError);
    }

    addByStream(p: PlaylistPostModel): Observable<PlaylistModel> {
        return this.http.post(`${variables.BASE_API_URL}${this.BaseUrl}/create/m3u`, p,
            { headers: new HttpHeaders().set('Content-Type', 'application/json'), responseType: 'text' }).catch(this.handleError);
    }

    update(p: PlaylistModel): Observable<PlaylistModel> {
        return this.http.put(`${variables.BASE_API_URL}${this.BaseUrl}/${p.publicId}`, p,
            { headers: new HttpHeaders().set('Content-Type', 'application/json'), responseType: 'text' }).catch(this.handleError);
    }

    updateLight(p: PlaylistModel): Observable<any> {
        return this.http.put(`${variables.BASE_API_URL}${this.BaseUrl}/light/${p.publicId}`, p,
            { headers: new HttpHeaders().set('Content-Type', 'application/json'), responseType: 'text' })
            .catch(this.handleError);
    }
}