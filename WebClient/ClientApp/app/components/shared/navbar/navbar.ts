import { Component, NgModule, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MdButtonModule, MdMenuModule, MdDialogRef, MD_DIALOG_DATA, MdSnackBar, MdDialog } from '@angular/material';
import { RouterModule } from '@angular/router';
import { AppModuleMaterialModule } from '../../../app.module.material.module';
import { AuthResponse, User } from '../../../types/auth.type';
import { AuthService } from '../../../services/auth/auth.service';
import { Observable } from "rxjs/Observable";
import './navbar.scss';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import 'rxjs/add/operator/first';
import 'rxjs/add/operator/take';

@Component({
    selector: 'app-navbar',
    templateUrl: './navbar.html',
})
export class NavBar implements OnInit {

    isAuthenticated: BehaviorSubject<boolean>;
    user: BehaviorSubject<User>;

    constructor(private authService: AuthService) {
        this.isAuthenticated = this.authService.authenticated;
        this.user = this.authService.user;
    }

    ngOnInit(): void {
        this.authService.isAuthenticated();
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