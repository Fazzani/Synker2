import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { MatButtonModule, MatMenuModule, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar, MatDialog, MatDialogConfig } from '@angular/material';
import { AuthService } from '../../../services/auth/auth.service';
import { Login, User, RegisterUser } from '../../../types/auth.type';
import { CommonService } from '../../../services/common/common.service';
import { HttpErrorResponse } from '@angular/common/http';

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

            data.genders = [
                {
                    value: 0, viewValue: "Mr"
                },
                {
                    value: 1, viewValue: "Mrs"
                }];

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
export class LoginDialog implements OnInit, OnDestroy {


    constructor(public dialogRef: MatDialogRef<LoginDialog>, private authService: AuthService, private router: Router, private commonService: CommonService) {
    }

    login(user: Login): void {
        if (user != null) {
            this.authService.Signin(user).subscribe(res => {
                //console.log(`${res.accessToken} refreshToken ${res.refreshToken}`);
                this.dialogRef.close(true);
                this.router.navigateByUrl(this.authService.redirectUrl);
            },
                (err: HttpErrorResponse) => {
                    if (err.status == 401)
                        this.commonService.displayError('Logon Failure', 'Logon Failure Unknown username or bad password');
                    else
                        this.commonService.displayError('Logon Failure', err.error);
                });
        }
    }

    ngOnInit(): void {
        if (this.authService.authenticated.getValue()) {
            this.dialogRef.close(true);

            if (this.authService.redirectUrl)
                this.router.navigateByUrl(this.authService.redirectUrl);
            else
                this.router.navigate(['home']);
        }
    }

    ngOnDestroy(): void {
    }
}

@Component({
    selector: 'register-dialog',
    templateUrl: '../../../components/auth/register.dialog.html'
})
export class RegisterDialog {
    photo: any;

    constructor( @Inject(MAT_DIALOG_DATA) public data: any, public dialogRef: MatDialogRef<RegisterDialog>, private authService: AuthService
        , private commonService: CommonService) {
    }

    register(registerUser: RegisterUser): void {
        if (registerUser != null)
            this.authService.Register(registerUser).subscribe(res => {
                //console.log(`${res.accessToken} refreshToken ${res.refreshToken}`);
                this.dialogRef.close(true);
            },
                err => this.commonService.displayError('Registration Failure', err.error))
    }

    changeListener($event): void {
        this.readThis($event.target);
    }

    readThis(inputValue: any): void {
        var file: File = inputValue.files[0];
        var myReader: FileReader = new FileReader();

        myReader.onloadend = (e) => {
            this.photo = myReader.result;
        }
        myReader.readAsDataURL(file);
    }
}