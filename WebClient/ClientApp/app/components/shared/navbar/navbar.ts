import { Component, NgModule, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
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
import { EqualValidator } from '../../../directives/equal-validator.directive';
import { MessageService } from '../../../services/message/message.service';

@Component({
    selector: 'app-navbar',
    templateUrl: './navbar.html',
})
export class NavBar implements OnInit {

    isAuthenticated: BehaviorSubject<boolean>;
    user: BehaviorSubject<User>;

    constructor(private authService: AuthService, private messageService: MessageService) {
        this.isAuthenticated = this.authService.authenticated;
        this.user = this.authService.user;
        this.authService.connect();
    }

    ngOnInit(): void {
        this.user.subscribe(user => {
            if (user != undefined) {
                console.log(`User ${user.firstName} is authenticated...`);
                this.messageService.listByStatus(0, 0, 10).subscribe(msg => {
                    console.log(msg);
                });
            }
        });

    }

    signout(): void {
        this.authService.signout();
    }

}

@NgModule({
    imports: [MdButtonModule, MdMenuModule, RouterModule, FormsModule, AppModuleMaterialModule, CommonModule, ReactiveFormsModule],
    exports: [NavBar, EqualValidator],
    declarations: [NavBar, EqualValidator]

})
export class NavBarModule {
}