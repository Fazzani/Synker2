import { Component, OnInit, OnDestroy } from "@angular/core";
import { MatSnackBar } from "@angular/material";
import { UsersService } from "../../services/admin/users.service";
//import { AuthService } from "../../services/auth/auth.service";
import { User } from "../../types/auth.type";

@Component({
  selector: "user",
  templateUrl: "./user.component.html"
})
export class UserComponent implements OnInit, OnDestroy {
  genders: { value: number; viewValue: string }[];

  user: User = <User>{};
  constructor(
    //private authService: AuthService,
    private usersService: UsersService,
    private snackBar: MatSnackBar) { }

  ngOnInit(): void {
    this.genders = User.GENDERS;
    this.usersService.me().subscribe(x => (this.user = x));
  }

  save(): void {
   // this.usersService.update(this.user).subscribe(x => this.authService.user.next(this.user));
  }

  changeListener($event): void {
    this.readThis($event.target);
  }

  readThis(inputValue: any): void {
    var file: File = inputValue.files[0];
    var myReader: FileReader = new FileReader();

    myReader.onloadend = e => {
      this.user.photo = myReader.result;
    };
    myReader.readAsDataURL(file);
  }

  ngOnDestroy() {}
}
