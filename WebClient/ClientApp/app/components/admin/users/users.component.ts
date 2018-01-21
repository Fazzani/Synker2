import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { MatDialog, MatSnackBar, MatTableDataSource, MatPaginator, MatSort } from '@angular/material';
import { Observable } from 'rxjs/Observable';
import { PagedResult, QueryListBaseModel } from '../../../types/common.type';
import { User } from '../../../types/auth.type';
import { UsersService } from '../../../services/admin/users.service';
import { CommonService } from '../../../services/common/common.service';

@Component({
    selector: 'users',
    templateUrl: './users.component.html'
})
export class UsersComponent implements OnInit, OnDestroy {
    columns = [
        { columnDef: 'id', header: 'Id', cell: (row: User) => `${row.id}`, showed: true, actionColumn: false },
        { columnDef: 'photo', header: 'Photo', cell: (row: User) => `${row.photo}`, showed: true, actionColumn: false, isImage: true },
        { columnDef: 'firstName', header: 'FirstName', cell: (row: User) => `${row.firstName}`, showed: true, actionColumn: false },
        { columnDef: 'lastName', header: 'LastName', cell: (row: User) => `${row.lastName}`, showed: true, actionColumn: false },
        { columnDef: 'email', header: 'Email', cell: (row: User) => `${row.email}`, showed: true, actionColumn: false },
        { columnDef: 'roles', header: 'Roles', cell: (row: User) => `${row.roles}`, showed: true, actionColumn: false },
        { columnDef: 'actions', header: 'Actions', cell: (row: User) => ``, showed: true, actionColumn: true }
    ];

    columnDefs = this.columns.filter(x => !x.actionColumn && x.showed);

    displayedColumns = this.columns.map(x => x.columnDef);
    dataSource: MatTableDataSource<User>;
    @ViewChild(MatPaginator) paginator: MatPaginator;
    @ViewChild(MatSort) sort: MatSort;
    @ViewChild('filter') filter: ElementRef;
    users: PagedResult<User>;
    query: QueryListBaseModel;

    constructor(private commonService: CommonService, public snackBar: MatSnackBar, private usersService: UsersService) { }

    ngOnInit(): void {
        this.dataSource = new MatTableDataSource<User>([]);
        this.paginator.pageSizeOptions = [50, 100, 250, 1000];
        this.dataSource = new MatTableDataSource<User>([]);

        this.query = <QueryListBaseModel>{ pageNumber: 0, pageSize: 10000000 };

        this.usersService.list(this.query).subscribe(res => {
            this.dataSource = new MatTableDataSource<User>(res.results);
        });
    }

    trackById = (index, item) => +item.id;

    reload(): void {
        this.ngOnInit();
    }

    ngOnDestroy() {
    }
}
