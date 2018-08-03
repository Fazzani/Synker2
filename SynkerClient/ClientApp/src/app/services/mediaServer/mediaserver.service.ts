import { catchError, map } from 'rxjs/operators';
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import { Observable } from "rxjs";
import { MediaServerStreamsStats } from '../../types/mediaserver.streams.stats.type';
import { MediaServerStats } from '../../types/mediaserver.stats.type';
import { MediaServerOptions, MediaServerLiveResponse } from '../../types/mediaServerConfig.type';

@Injectable({
  providedIn: 'root',
})
export class MediaServerService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http);
    this._baseUrl = 'mediaserver';
  }

  public server(): Observable<MediaServerStats> {
    return this.http
      .get(`${this.FullBaseUrl}/server`).pipe(
      map(this.handleSuccess),
      catchError(this.handleError));
  }

  public streams(): Observable<MediaServerStreamsStats> {
     return this.http
      .get(`${this.FullBaseUrl}/streams`).pipe(
      map(this.handleSuccess),
      catchError(this.handleError));
  }

  public config(): Observable<MediaServerOptions> {
    return this.http
      .get(`${this.FullBaseUrl}/config`).pipe(
        map(this.handleSuccess),
        catchError(this.handleError));
  }

  public live(stream: string): Observable<MediaServerLiveResponse> {
    return this.http
      .post(`${this.FullBaseUrl}/live`, { stream: stream }).pipe(
        map(this.handleSuccess),
        catchError(this.handleError));
  }

  public stop(id: string): Observable<MediaServerLiveResponse> {
    return this.http
      .post(`${this.FullBaseUrl}/stop`, { stream: id }).pipe(
        map(this.handleSuccess),
        catchError(this.handleError));
  }
}
