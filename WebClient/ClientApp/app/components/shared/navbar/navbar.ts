import { Component, NgModule, OnInit, Inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule, MatMenuModule, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar, MatDialog } from '@angular/material';
import { RouterModule } from '@angular/router';
import { AppModuleMaterialModule } from '../../../app.module.material.module';
import { AuthResponse, User } from '../../../types/auth.type';
import { AuthService } from '../../../services/auth/auth.service';
import { Observable } from "rxjs/Observable";
import './navbar.scss';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { first, take } from 'rxjs/operators';
import { EqualValidator } from '../../../directives/equal-validator.directive';
import { MessageService } from '../../../services/message/message.service';
import { Subscription } from 'rxjs/Subscription';
import { PagedResult } from '../../../types/elasticQuery.type';
import { Message } from '../../../types/message.type';

@Component({
    selector: 'app-navbar',
    templateUrl: './navbar.html',
})
export class NavBar implements OnInit, OnDestroy {

    isAuthenticated: BehaviorSubject<boolean>;
    user: BehaviorSubject<User>;
    userSubscription: Subscription;
    messages: PagedResult<Message>;

    constructor(private authService: AuthService, private messageService: MessageService) {
        this.isAuthenticated = this.authService.authenticated;
        this.user = this.authService.user;
        this.authService.connect();
    }

    ngOnInit(): void {
        this.userSubscription = this.user.subscribe(user => {
            if (user != undefined) {
                console.log(`User ${user.firstName} is authenticated...`);
                this.messageService.listByStatus(0, 0, 10).subscribe(msg => {
                    this.messages = msg;
                });
            }
        });
    }

    signout(): void {
        this.isAuthenticated.next(false);
        this.authService.signout();
    }

    ngOnDestroy(): void {

        this.userSubscription.unsubscribe();
    }

}

@NgModule({
    imports: [MatButtonModule, MatMenuModule, RouterModule, FormsModule, AppModuleMaterialModule, CommonModule, ReactiveFormsModule],
    exports: [NavBar, EqualValidator],
    declarations: [NavBar, EqualValidator]

})
export class NavBarModule {
}