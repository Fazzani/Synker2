﻿import { MAT_DIALOG_DATA, MatDialogRef, MatDialog, MatSnackBar, MatDialogConfig } from "@angular/material";
import { Inject, Component, OnInit } from "@angular/core";
import { AuthService } from "../../../services/auth/auth.service";
import { CommonService } from "../../../services/common/common.service";
import { RegisterUser } from "../../../types/auth.type";
import { ActivatedRoute, Router } from "@angular/router";

@Component({
    selector: 'register-dialog',
    templateUrl: './register.dialog.html'
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


@Component({
    selector: 'register-component',
    templateUrl: '../../shared/dialogs/dialog.component.html'
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