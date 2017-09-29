import { Component, NgModule, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MdButtonModule, MdMenuModule, MdDialogRef, MD_DIALOG_DATA, MdSnackBar, MdDialog } from '@angular/material';
import { RouterModule } from '@angular/router';
import { AuthResponse, User } from '../../../types/auth.type';
import { AppModuleMaterialModule } from '../../../app.module.material.module';
import { Observable } from "rxjs/Observable";
import './navbar.scss';
import { AuthService } from '../../../services/auth/auth.service';

@Component({
    selector: 'app-navbar',
    templateUrl: './navbar.html',
})
export class NavBar implements OnInit {
    isAuthenticated: boolean;
    user: User;

    color: string = "primary";

    constructor(private authService: AuthService) {

    }

    ngOnInit(): void {
        this.user = this.authService.getUser();
        this.isAuthenticated = this.authService.isAuthenticated();
    }

    signout(): void {
        this.authService.signout();
    }

}

@NgModule({
    imports: [MdButtonModule, MdMenuModule, RouterModule, FormsModule, AppModuleMaterialModule, CommonModule],
    exports: [NavBar],
    declarations: [NavBar]
})
export class NavBarModule {
}