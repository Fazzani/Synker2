import { Injectable } from "@angular/core";
import { catchError, map } from "rxjs/operators";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import { Observable } from "rxjs";
import { PagedResult, QueryListBaseModel } from "../../types/common.type";
import { Device, DeviceModel } from "../../types/device.type";

@Injectable({
  providedIn: "root"
})
export class DeviceService extends BaseService {
  constructor(protected http: HttpClient) {
    super(http);
    this._baseUrl = "devices";
  }

  public get(id: string): Observable<PagedResult<Device>> {
    return this.http.get(`${this.FullBaseUrl}/${id}`).pipe(
      map(this.handleSuccess)
    );
  }

  public update(device: DeviceModel): Observable<number> {
    return this.http.put(`${this.FullBaseUrl}/${device.id}`, device).pipe(
      map(this.handleSuccess)
    );
  }

  public create(pushSubscription: PushSubscription): Observable<number> {

    return this.http.post(`${this.FullBaseUrl}`, pushSubscription.toJSON()).pipe(
      map(this.handleSuccess)
    );
  }

  public list(page: number, pageSize: number): Observable<PagedResult<Device>> {
    return this.http
      .post(`${this.FullBaseUrl}/search`, <QueryListBaseModel>{
        pageNumber: page,
        pageSize: pageSize
      })
      .pipe(
        map(this.handleSuccess)
      );
  }
}
