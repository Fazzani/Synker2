import { Component, NgModule, OnInit, Inject, OnDestroy, HostBinding, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule, MatMenuModule, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar, MatDialog } from '@angular/material';
import { RouterModule } from '@angular/router';
import { AppModuleMaterialModule } from '../../../app.module.material.module';
import { AuthResponse, User } from '../../../types/auth.type';
import { AuthService } from '../../../services/auth/auth.service';
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { first, take } from 'rxjs/operators';
import { EqualValidator } from '../../../directives/equal-validator.directive';
import { MessageService } from '../../../services/message/message.service';
import { Subscription } from 'rxjs/Subscription';
import { Message, MessageStatus } from '../../../types/message.type';
import { PagedResult } from '../../../types/common.type';
import { OverlayContainer } from '@angular/cdk/overlay';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.scss']
})
export class NavBar implements OnInit, OnDestroy {

  isAuthenticated: BehaviorSubject<boolean>;
  user: BehaviorSubject<User>;
  userSubscription: Subscription;
  messages: PagedResult<Message>;
  @Output() onThemeChanged = new EventEmitter();

  constructor(private authService: AuthService, private messageService: MessageService) {
    this.isAuthenticated = this.authService.authenticated;
    this.user = this.authService.user;
    this.authService.connect();
  }

  ngOnInit(): void {
    this.userSubscription = this.user.subscribe(user => {
      if (user != undefined) {
        console.log(`User ${user.firstName} is authenticated...`);
        this.messageService.listByStatus([MessageStatus.None, MessageStatus.NotReaded], 0, 10).subscribe(msg => {
          this.messages = msg;
        });
      }
    });
  }

  onSetTheme(theme) {
    this.onThemeChanged.emit(theme);
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
