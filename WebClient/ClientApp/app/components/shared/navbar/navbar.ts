import { Component, NgModule, OnInit, Inject } from '@angular/core';
import { MdButtonModule, MdMenuModule, MdIconModule, MdDialogRef, MD_DIALOG_DATA } from '@angular/material';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth/auth.service';

import './navbar.scss';

@Component({
    selector: 'app-navbar',
    templateUrl: './navbar.html',
    providers: [AuthService]
})
export class NavBar implements OnInit {

    color: string = "primary";

    constructor(private authService: AuthService) {

    }

    ngOnInit(): void {
        var login = { username: "tunisienheni@outlook.com", password: "password2" };

        this.authService.Signin(login.username, login.password).map(res => { console.log(res); });
    }
}

@Component({
    selector: 'login-dialog',
    templateUrl: './login.dialog.html'
})
export class LoginDialog {

    constructor(
        public dialogRef: MdDialogRef<any>,
        @Inject(MD_DIALOG_DATA) public data: any) { }

    onNoClick(): void {
        this.dialogRef.login();
    }

}

@NgModule({
    imports: [MdButtonModule, MdMenuModule, MdIconModule, RouterModule],
    exports: [NavBar],
    declarations: [NavBar],
})
export class NavBarModule {


}