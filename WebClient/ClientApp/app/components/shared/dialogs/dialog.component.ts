import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { MdButtonModule, MdMenuModule, MdDialogRef, MD_DIALOG_DATA, MdSnackBar, MdDialog, MdDialogConfig } from '@angular/material';
import { AuthService } from '../../../services/auth/auth.service';
import { Login, User, RegisterUser } from '../../../types/auth.type';

@Component({
    selector: 'synker-dialog',
    templateUrl: './dialog.component.html',
    styleUrls: ['./dialog.component.css'],
    providers: [AuthService]
})
export class DialogComponent implements OnInit {

    constructor(private dialog: MdDialog, private authService: AuthService, public snackBar: MdSnackBar, private activatedRoute: ActivatedRoute) {
    }

    ngOnInit() {
        this.activatedRoute.params.subscribe(params => {
            if (params["modal"] == 'true' && params["dialog"] == 'signin') {
                this.openLoginDialog();
            }
            else
                if (params["modal"] == 'true' && params["dialog"] == 'register') {
                    this.openRegisterDialog();
                }
        });
    }

    /**
     * Open login dialog
     * 
     * @memberof DialogComponent
     */
    openLoginDialog(): void {
        let data = <Login>{};
        setTimeout(() => {
            let dialogRef = this.dialog.open(LoginDialog, <MdDialogConfig>{
                disableClose: true,
                data: data
            }).afterClosed().subscribe(result => {
                if (result) {

                }
            });
        });
    }

    /**
     * Open Register dialog
     * 
     * @memberof DialogComponent
     */
    openRegisterDialog(): void {

        let data = <RegisterUser>{};
        data.genders = [{ value: 0, viewValue: "Mr" }, { value: 0, viewValue: "Mrs" }];
        setTimeout(() => {
            let dialogRef = this.dialog.open(RegisterDialog, <MdDialogConfig>{
                disableClose: true,
                data: data
            }).afterClosed().subscribe(result => {
                if (result) {

                }
            });
        });
    }
}


@Component({
    selector: 'login-dialog',
    templateUrl: '../../../components/auth/login.dialog.html'
})
export class LoginDialog {

    constructor(public dialogRef: MdDialogRef<LoginDialog>, private authService: AuthService) {
    }

    login(user: Login): void {
        if (user != null)
            this.authService.Signin(user.username, user.password).subscribe(res => {
                console.log(`${res.accessToken} refreshToken ${res.refreshToken}`);
                this.dialogRef.close(true);
            },
                err => console.log(err))
    }
}

@Component({
    selector: 'regiser-dialog',
    templateUrl: '../../../components/auth/register.dialog.html'
})
export class RegisterDialog {

    constructor( @Inject(MD_DIALOG_DATA) public data: any, public dialogRef: MdDialogRef<RegisterDialog>, private authService: AuthService) {
    }

    register(registerUser: RegisterUser): void {
        if (registerUser != null)
            this.authService.Register(registerUser).subscribe(res => {
                console.log(`${res.accessToken} refreshToken ${res.refreshToken}`);
                this.dialogRef.close(true);
            },
                err => console.log(err.error))
    }
}