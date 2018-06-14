import { Observable } from "rxjs";
import { map, catchError } from 'rxjs/operators';
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import { PagedResult, QueryListBaseModel } from "../../types/common.type";
import { Host } from "../../types/host.type";

@Injectable({
  providedIn: 'root',
})
export class HostsService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http, "host");
  }

  update(host: Host): Observable<Host> {
    return this.http
      .put(`${this.FullBaseUrl}/${host.id}`, host).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  public get(id: number): Observable<Host> {
    return this.http
      .get(`${this.FullBaseUrl}/${id}`).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  public list(queryModel: QueryListBaseModel): Observable<PagedResult<Host>> {
    return this.http
      .post(`${this.FullBaseUrl}/search`, queryModel).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }
}
