import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from "@angular/core";
import { MatSnackBar, MatTableDataSource, MatPaginator, MatSort } from "@angular/material";
import { Observable } from "rxjs/Rx";
import { QueryListBaseModel } from "../../../types/common.type";
import { Host } from "../../../types/host.type";
import { HostsService } from "../../../services/admin/hosts.service";

@Component({
  selector: "hosts",
  templateUrl: "./hosts.component.html"
  //animations: [slideInDownAnimation]
})
export class HostsComponent implements OnInit, OnDestroy {
  columns = [
    {
      columnDef: "id",
      header: "Id",
      cell: (row: Host) => `${row.id}`,
      showed: true,
      actionColumn: false
    },
    {
      columnDef: "name",
      header: "Name",
      cell: (row: Host) => `${row.name}`,
      showed: true,
      actionColumn: false,
      isImage: false
    },
    {
      columnDef: "addressUri",
      header: "Address",
      cell: (row: Host) => `${row.addressUri}`,
      showed: true,
      actionColumn: false
    },
    {
      columnDef: "enabled",
      header: "Enabled",
      cell: (row: Host) => `${row.enabled}`,
      showed: true,
      actionColumn: false
    },
    {
      columnDef: "actions",
      header: "Actions",
      cell: (row: Host) => ``,
      showed: true,
      actionColumn: true
    }
  ];

  columnDefs = this.columns.filter(x => !x.actionColumn && x.showed);

  displayedColumns = this.columns.map(x => x.columnDef);
  dataSource: MatTableDataSource<Host>;
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;
  @ViewChild("filter") filter: ElementRef;
  query: QueryListBaseModel;

  constructor(private hostsService: HostsService, public snackBar: MatSnackBar) {}

  ngOnInit(): void {
    this.dataSource = new MatTableDataSource<Host>([]);
    this.paginator.pageSizeOptions = [50, 100, 250];
    this.query = <QueryListBaseModel>{ getAll: true };

    this.hostsService.list(this.query).subscribe(res => {
      this.dataSource = new MatTableDataSource<Host>(res.results);
    });
  }

  trackById = (index, item) => +item.id;

  reload(): void {
    this.ngOnInit();
  }

  delete(id: number): void {
    let hostIndex = this.dataSource.data.findIndex(x => x.id == id);
    const confirm = window.confirm(`Do you really want to delete ${this.dataSource.data[hostIndex].name}?`);

    Observable.of(id)
      .filter(() => confirm)
      .switchMap(x => this.hostsService.delete(x.toString()))
      .subscribe(res => this.snackBar.open("The Host deleted successfully"));
  }

  ngOnDestroy() {}
}
