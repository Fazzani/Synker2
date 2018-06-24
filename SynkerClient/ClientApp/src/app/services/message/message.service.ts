import { Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../base/base.service';
import { Message, MessageQueryModel } from '../../types/message.type';
import { PagedResult } from '../../types/common.type';

@Injectable({
  providedIn: 'root',
})
export class MessageService extends BaseService {
  url = `${this.FullBaseUrl}/status/`;

  constructor(protected http: HttpClient) {
    super(http);
    this._baseUrl = 'message';
  }

  public get(id: string): Observable<PagedResult<Message>> {
    return this.http
      .get(this.url + id).pipe(
      map(this.handleSuccess),
      catchError(this.handleError));
  }

  public update(message: Message): Observable<number> {
    return this.http
      .put(`${this.FullBaseUrl}/${message.id}`, message).pipe(
      map(this.handleSuccess),
      catchError(this.handleError));
  }

  public list(): Observable<PagedResult<Message>> {
    return this.http
      .get(this.url).pipe(
      map(this.handleSuccess),
      catchError(this.handleError));
  }

  public listByStatus(status: number[], page: number, pageSize: number): Observable<PagedResult<Message>> {
    return this.http
      .post(`${this.FullBaseUrl}/search/status`, <MessageQueryModel>{
        MessageStatus: status,
        pageNumber: page,
        pageSize: pageSize
      }).pipe(
      map(this.handleSuccess),
      catchError(this.handleError));
  }
}
