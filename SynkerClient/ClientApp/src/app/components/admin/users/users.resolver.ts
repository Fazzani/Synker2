import { Resolve } from "@angular/router";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Injectable } from "@angular/core";
import { User } from "../../../types/auth.type";
import { MatTableDataSource } from "@angular/material";
import { QueryListBaseModel } from "../../../types/common.type";
import { UsersService } from "../../../services/admin/users.service";
import { map } from "rxjs/operators";

@Injectable()
export class UsersResolver implements Resolve<any> {
  constructor(private usersService: UsersService) {}

  resolve(route: ActivatedRouteSnapshot) {
    return this.usersService.list(<QueryListBaseModel>{ getAll: true }).pipe(map(res => new MatTableDataSource<User>(res.results)));
  }
}
