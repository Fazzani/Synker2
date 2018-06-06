import { Component, OnInit } from "@angular/core";
import { MatSnackBar, MatDialog, MatDialogConfig } from "@angular/material";
import { Login } from "../../../types/auth.type";
import { LoginDialog } from "../../dialogs/auth/LoginDialog";

@Component({
  selector: "synker-dialog",
  templateUrl: "./dialog.component.html",
  styleUrls: ["./dialog.component.css"]
})
export class DialogComponent implements OnInit {
  constructor(private dialog: MatDialog, public snackBar: MatSnackBar) {}

  ngOnInit() {
    let data = <Login>{};
    setTimeout(() => {
      let dialogRef = this.dialog
        .open(LoginDialog, <MatDialogConfig>{
          disableClose: true,
          data: data
        })
        .afterClosed()
        .subscribe(result => {
          //if (result == '') {
          //    this.router.navigateByUrl('/');
          //}
        });
    });
  }
}
