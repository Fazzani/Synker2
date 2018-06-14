import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from "@angular/core";
import { MatSnackBar, MatTableDataSource, MatPaginator, MatSort } from "@angular/material";
import { of } from "rxjs";
import { switchMap, filter} from 'rxjs/operators';
import { QueryListBaseModel } from "../../../types/common.type";
import { Host } from "../../../types/host.type";
import { HostsService } from "../../../services/admin/hosts.service";
import { ActivatedRoute } from "@angular/router";

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

  constructor(private route: ActivatedRoute, private hostsService: HostsService, public snackBar: MatSnackBar) {}

  ngOnInit(): void {
    this.dataSource = new MatTableDataSource<Host>([]);
    this.paginator.pageSizeOptions = [50, 100, 250];
    this.query = <QueryListBaseModel>{ getAll: true };

    this.dataSource = <MatTableDataSource<Host>>this.route.snapshot.data.data;
  }

  trackById = (index, item) => +item.id;

  reload(): void {
    this.ngOnInit();
  }

  delete(id: number): void {
    let hostIndex = this.dataSource.data.findIndex(x => x.id == id);
    const confirm = window.confirm(`Do you really want to delete ${this.dataSource.data[hostIndex].name}?`);

    of(id).pipe(
      filter(() => confirm),
      switchMap(x => this.hostsService.delete(x.toString())),)
      .subscribe(res => {
        this.snackBar.open("The Host deleted successfully");
        this.ngOnInit();
      });
  }

  ngOnDestroy() {}
}
