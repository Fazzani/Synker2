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
        let dialogRef = this.dialog.open(LoginDialog, {
            // width: '550px',
            // height: '500px',
            data: data
        });

        dialogRef.afterClosed().subscribe(result => {
            this.authService.Signin(data.username, data.password).subscribe(res => {
                this.snackBar.open(`${res.accessToken} refreshToken ${res.refreshToken}`);
                console.log(`${res.accessToken} refreshToken ${res.refreshToken}`);
            },
                err => console.log(err))
        });
    }
}


@Component({
    selector: 'login-dialog',
    templateUrl: '../../../components/auth/login.dialog.html'
})
export class LoginDialog {

    constructor(
        public dialogRef: MdDialogRef<any>,
        @Inject(MD_DIALOG_DATA) public data: any) { }

    onNoClick(): void {
        this.dialogRef.close();
    }

}