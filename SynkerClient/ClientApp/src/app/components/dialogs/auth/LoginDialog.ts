import { MatDialogRef } from "@angular/material";
import { AuthService } from "../../../services/auth/auth.service";
import { Router } from "@angular/router";
import { CommonService } from "../../../services/common/common.service";
import { OnInit, OnDestroy, Component } from "@angular/core";
import { Login } from "../../../types/auth.type";
import { HttpErrorResponse } from "@angular/common/http";
import { debug } from "util";

@Component({
  selector: "login-dialog",
  templateUrl: "./login.dialog.html"
})
export class LoginDialog implements OnInit, OnDestroy {
  public user: Login = <Login>{};

  constructor(public dialogRef: MatDialogRef<LoginDialog>, private authService: AuthService, private router: Router, private commonService: CommonService) {}

  login(): void {
    if (this.user != null) {
      this.authService.Signin(this.user).subscribe(
        res => {
          //console.log(`${res.accessToken} refreshToken ${res.refreshToken}`);
          this.dialogRef.close(true);
          this.router.navigateByUrl(this.authService.redirectUrl);
        },
        (err: HttpErrorResponse) => {
          if (err.status == 401) this.commonService.displayError("Logon Failure", "Logon Failure Unknown username or bad password");
          else this.commonService.displayError("Logon Failure", err.error);
        }
      );
    }
  }

  ngOnInit(): void {
    if (this.authService.authenticated.getValue()) {
      this.dialogRef.close(true);

      if (this.authService.redirectUrl) this.router.navigateByUrl(this.authService.redirectUrl);
      else this.router.navigate(["home"]);
    }
  }

  ngOnDestroy(): void {}
}
