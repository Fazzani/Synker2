import { Component, OnInit, OnDestroy } from "@angular/core";
import { MatSnackBar } from "@angular/material";
import { UsersService } from "../../services/admin/users.service";
//import { AuthService } from "../../services/auth/auth.service";
import { User } from "../../types/auth.type";
import { ActivatedRoute } from '@angular/router';
import { FormControl } from '@angular/forms';
import * as moment from 'moment';

@Component({
  selector: "user",
  templateUrl: "./user.component.html"
})
export class UserComponent implements OnInit, OnDestroy {
  genders: string[];
  birthdate = new FormControl(moment());

  user: User = <User>{};
  constructor(
    private route: ActivatedRoute,
    private usersService: UsersService,
    private snackBar: MatSnackBar) { }

  ngOnInit(): void {
    this.genders = User.GENDERS;
    this.user = <User>this.route.snapshot.data.user;
    this.birthdate.setValue(this.user.birthdate);
  }

  ngOnDestroy() {}
}
