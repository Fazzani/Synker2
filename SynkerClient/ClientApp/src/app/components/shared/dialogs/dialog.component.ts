import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { MatButtonModule, MatMenuModule, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar, MatDialog, MatDialogConfig } from '@angular/material';
import { AuthService } from '../../../services/auth/auth.service';
import { Login, User, RegisterUser } from '../../../types/auth.type';
import { CommonService } from '../../../services/common/common.service';
import { HttpErrorResponse } from '@angular/common/http';
import { LoginDialog } from '../../dialogs/auth/LoginDialog';

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



