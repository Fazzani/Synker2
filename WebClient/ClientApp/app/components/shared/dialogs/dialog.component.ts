import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { MdButtonModule, MdMenuModule, MdDialogRef, MD_DIALOG_DATA, MdSnackBar, MdDialog } from '@angular/material';
import { AuthService } from '../../../services/auth/auth.service';

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
        });
    }

    openLoginDialog(): void {
        let data = { username: '', password: '' };
        setTimeout(() => {
            let dialogRef = this.dialog.open(LoginDialog, {
                // width: '550px',
                // height: '500px',
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

    login(data: any): void {
        if (data != '')
            this.authService.Signin(data.value.username, data.value.password).subscribe(res => {
                console.log(`${res.accessToken} refreshToken ${res.refreshToken}`);
                this.dialogRef.close(true);
            },
                err => console.log(err))
    }
}