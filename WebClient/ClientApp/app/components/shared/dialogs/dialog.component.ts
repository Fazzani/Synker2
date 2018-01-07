import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { MatButtonModule, MatMenuModule, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar, MatDialog, MatDialogConfig } from '@angular/material';
import { AuthService } from '../../../services/auth/auth.service';
import { Login, User, RegisterUser } from '../../../types/auth.type';

@Component({
    selector: 'synker-dialog',
    templateUrl: './dialog.component.html',
    styleUrls: ['./dialog.component.css']
})
export class DialogComponent implements OnInit {

    constructor(private dialog: MatDialog, public snackBar: MatSnackBar, private activatedRoute: ActivatedRoute, private router: Router) {
    }

    ngOnInit() {
        let data = <Login>{};
        setTimeout(() => {
            let dialogRef = this.dialog.open(LoginDialog, <MatDialogConfig>{
                disableClose: true,
                data: data
            }).afterClosed().subscribe(result => {
                //if (result == '') {
                //    this.router.navigateByUrl('/');

                //}
            });
        });
    }
}

@Component({
    selector: 'register-component',
    templateUrl: './dialog.component.html'
})
export class RegisterComponent implements OnInit {

    constructor(private dialog: MatDialog, public snackBar: MatSnackBar, private activatedRoute: ActivatedRoute, private router: Router) {
    }

    ngOnInit() {
        this.activatedRoute.queryParamMap.subscribe(params => {
            let data = <RegisterUser>{};
            data.genders = [{ value: 0, viewValue: "Mr" }, { value: 0, viewValue: "Mrs" }];
            setTimeout(() => {
                let dialogRef = this.dialog.open(RegisterDialog, <MatDialogConfig>{
                    disableClose: true,
                    data: data
                }).afterClosed().subscribe(result => {
                    if (result == '') {
                        this.router.navigateByUrl('/');
                    }
                });
            });
        });
    }
}

@Component({
    selector: 'login-dialog',
    templateUrl: '../../../components/auth/login.dialog.html'
})
export class LoginDialog {

    constructor(public dialogRef: MatDialogRef<LoginDialog>, private authService: AuthService, private router: Router) {
    }

    login(user: Login): void {
        if (user != null)
            this.authService.Signin(user).subscribe(res => {
                console.log(`${res.accessToken} refreshToken ${res.refreshToken}`);
                this.dialogRef.close(true);
            },
                err => console.log(err))
    }
}

@Component({
    selector: 'register-dialog',
    templateUrl: '../../../components/auth/register.dialog.html'
})
export class RegisterDialog {

    constructor( @Inject(MAT_DIALOG_DATA) public data: any, public dialogRef: MatDialogRef<RegisterDialog>, private authService: AuthService) {
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