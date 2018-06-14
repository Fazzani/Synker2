import { Resolve } from '@angular/router';
import { ActivatedRouteSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { QueryListBaseModel } from '../../../types/common.type';
import { HostsService } from '../../../services/admin/hosts.service';
import { map } from "rxjs/operators";
import { MatTableDataSource } from '@angular/material';
import { Host } from '../../../types/host.type';

@Injectable()
export class HostsResolver implements Resolve<any> {
  constructor(private hostsService: HostsService) {}

  resolve(route: ActivatedRouteSnapshot) {
    return this.hostsService.list(<QueryListBaseModel>{ getAll: true }).pipe(map(res => new MatTableDataSource<Host>(res.results)));
  }
}
