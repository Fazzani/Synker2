
import { of } from 'rxjs';
import { switchMap, filter} from 'rxjs/operators';
import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from "@angular/core";
import { MatSnackBar, MatTableDataSource, MatPaginator, MatSort } from "@angular/material";
import { PagedResult, QueryListBaseModel } from "../../../types/common.type";
import { User } from "../../../types/auth.type";
import { UsersService } from "../../../services/admin/users.service";
import { ActivatedRoute } from '@angular/router';
//import { slideInDownAnimation } from '../../animations';

@Component({
  selector: "users",
  templateUrl: "./users.component.html"
  // animations: [slideInDownAnimation]
})
export class UsersComponent implements OnInit, OnDestroy {
  columns = [
    { columnDef: "id", header: "Id", cell: (row: User) => `${row.id}`, showed: true, actionColumn: false },
    { columnDef: "photo", header: "Photo", cell: (row: User) => `${row.photo}`, showed: true, actionColumn: false, isImage: true },
    { columnDef: "firstName", header: "FirstName", cell: (row: User) => `${row.firstName}`, showed: true, actionColumn: false },
    { columnDef: "lastName", header: "LastName", cell: (row: User) => `${row.lastName}`, showed: true, actionColumn: false },
    { columnDef: "email", header: "Email", cell: (row: User) => `${row.email}`, showed: true, actionColumn: false },
    { columnDef: "roles", header: "Roles", cell: (row: User) => `${row.roles}`, showed: true, actionColumn: false },
    { columnDef: "actions", header: "Actions", cell: (row: User) => ``, showed: true, actionColumn: true }
  ];

  columnDefs = this.columns.filter(x => !x.actionColumn && x.showed);

  displayedColumns = this.columns.map(x => x.columnDef);
  dataSource: MatTableDataSource<User>;
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;
  @ViewChild("filter") filter: ElementRef;
  users: PagedResult<User>;
  query: QueryListBaseModel;

  constructor(private route: ActivatedRoute, public snackBar: MatSnackBar, private usersService: UsersService) {}

  ngOnInit(): void {
    this.dataSource = new MatTableDataSource<User>([]);
    this.paginator.pageSizeOptions = [50, 100, 250];

    this.query = <QueryListBaseModel>{ getAll: true };
    this.dataSource = <MatTableDataSource<User>>this.route.snapshot.data.data;
  }

  trackById = (index, item) => +item.id;

  reload(): void {
    this.ngOnInit();
  }

  delete(id: number): void {
    let userIndex = this.dataSource.data.findIndex(x => x.id == id);
    const confirm = window.confirm(`Do you really want to delete ${this.dataSource.data[userIndex].lastName}?`);

    of(id).pipe(
      filter(() => confirm),
      switchMap(x => this.usersService.delete(x.toString())),)
      .subscribe(res => this.snackBar.open("The user deleted successfully"));
  }

  ngOnDestroy() {}
}
