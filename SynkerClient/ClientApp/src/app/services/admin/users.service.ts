import { catchError, map } from 'rxjs/operators';
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import { Observable } from "rxjs";
import { User } from "../../types/auth.type";
import { PagedResult, QueryListBaseModel } from "../../types/common.type";

@Injectable({
  providedIn: 'root',
})
export class UsersService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http);
    this._baseUrl = "users";
  }

  update(user: User): Observable<User> {
    return this.http
      .put(`${this.FullBaseUrl}/${user.id}`, user).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  public me(): Observable<User> {
    return this.http
      .get(`${this.FullBaseUrl}`).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  public get(id: number): Observable<User> {
    return this.http
      .get(`${this.FullBaseUrl}/${id}`).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }

  public list(queryModel: QueryListBaseModel): Observable<PagedResult<User>> {
    return this.http
      .post(`${this.FullBaseUrl}/search`, queryModel).pipe(
      map(this.handleSuccess),
      catchError(this.handleError),);
  }
}
