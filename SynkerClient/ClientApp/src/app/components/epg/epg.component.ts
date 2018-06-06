import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject } from "@angular/core";
import { DataSource } from "@angular/cdk/collections";
import { MatPaginator, PageEvent, MatSort, MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from "@angular/material";
import { BehaviorSubject, Subscription } from "rxjs";

import { EpgService } from "../../services/epg/epg.service";
import { CommonService, Constants } from "../../services/common/common.service";
import { Observable } from "rxjs/Observable";
import { ElasticQuery } from "../../types/elasticQuery.type";
import { tvChannel } from "../../types/xmltv.type";
import "rxjs/add/observable/fromEvent";
import { map, catchError, merge, debounceTime, distinctUntilChanged } from "rxjs/operators";
import { snakbar_duration } from "../../variables";

@Component({
  selector: "epg-media",
  templateUrl: "./epg.component.html",
  providers: [EpgService, CommonService]
})
/** media component*/
export class EpgComponent implements OnInit, OnDestroy {
  subscriptionTableEvent: Subscription;

  displayedColumns = ["icon", "displayname", "id", "actions"];
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;
  @ViewChild("filter") filter: ElementRef;
  dataSource: EpgDataSource | null;
  currentItem: tvChannel | null;

  /** media ctor */
  constructor(private epgService: EpgService, private commonService: CommonService, public dialog: MatDialog, public snackBar: MatSnackBar) {}

  /** Called by Angular after media component initialized */
  ngOnInit(): void {
    let storedQuery = this.commonService.JsonToObject<ElasticQuery>(localStorage.getItem(Constants.LS_MediaQueryKey));
    console.log("storedQuery ", storedQuery);

    this.paginator.pageIndex = storedQuery != null ? Math.floor(storedQuery.from / storedQuery.size) : 0;
    this.paginator.pageSizeOptions = [50, 100, 250, 1000];
    this.paginator.pageSize = storedQuery != null ? storedQuery.size : this.paginator.pageSizeOptions[0];
    this.filter.nativeElement.value = storedQuery != null && storedQuery.query != null && storedQuery.query != {} ? JSON.stringify(storedQuery.query) : "";
    storedQuery = null;

    this.dataSource = new EpgDataSource(this.epgService, this.paginator);

    this.subscriptionTableEvent = this.paginator.page
      .asObservable()
      .merge(Observable.fromEvent<KeyboardEvent>(this.filter.nativeElement, "keyup"))
      .debounceTime(1000)
      .distinctUntilChanged()
      .subscribe(x => {
        if (!this.dataSource) {
          return;
        }
        console.log("subscriptionTableEvent => ", x);
        if ((x as PageEvent).length === undefined) this.paginator.pageIndex = 0;

        let objectQuery = this.commonService.JsonToObject(this.filter.nativeElement.value);
        console.log("objectQuery => ", objectQuery);
        this.dataSource.filter = objectQuery != null ? objectQuery : this.filter.nativeElement.value;
        this.dataSource.paginator = this.paginator;
      });
  }

  openDialog(epg: tvChannel): void {
    let dialogRef = this.dialog.open(EpgModifyDialog, {
      width: "550px",
      height: "500px",
      data: epg
    });

    dialogRef.afterClosed().subscribe(result => {
      this.snackBar.open(epg.displayname + " was modified", "", { duration: snakbar_duration });
    });
  }

  update(epg: tvChannel): void {
    console.log(epg);
  }

  ngOnDestroy() {
    this.subscriptionTableEvent.unsubscribe();
  }
}

@Component({
  selector: "epg-modify-dialog",
  templateUrl: "./epg.dialog.html"
})
export class EpgModifyDialog {
  constructor(public dialogRef: MatDialogRef<EpgModifyDialog>, @Inject(MAT_DIALOG_DATA) public data: any) {}

  onNoClick(): void {
    this.dialogRef.close();
  }
}

/**
 * Media datasource for mat-table component
 */
export class EpgDataSource extends DataSource<tvChannel> {
  medias = new BehaviorSubject<tvChannel[]>([]);

  _filterChange = new BehaviorSubject<Object | string>({});
  get filter(): Object | string {
    return this._filterChange.value;
  }
  set filter(filter: Object | string) {
    this._filterChange.next(filter);
  }

  _paginator = new BehaviorSubject<MatPaginator>(<MatPaginator>{});
  get paginator(): MatPaginator {
    return this._paginator.value;
  }
  set paginator(paginator: MatPaginator) {
    this._paginator.next(paginator);
  }

  constructor(private epgService: EpgService, private mdPaginator: MatPaginator) {
    super();

    this.paginator = mdPaginator;
    this._filterChange
      .merge(this._paginator)
      .debounceTime(300)
      //.distinctUntilChanged()
      .subscribe(x => this.getData());
  }

  connect(): Observable<tvChannel[]> {
    return this.medias.asObservable();
  }

  /**
   * Get medias list from webapi
   * @returns Obersvable<tvChannel[]>
   */
  getData(): Observable<tvChannel[]> {
    let pageSize = this.paginator.pageSize === undefined ? 25 : this.paginator.pageSize;
    let query = <ElasticQuery>{
      from: pageSize * this.paginator.pageIndex,
      size: pageSize
    };
    if (typeof this.filter === "string") {
      if (this.filter !== undefined && this.filter != "")
        query.query = {
          match: { _all: this.filter }
        };
    } else {
      query.query = this.filter;
    }
    localStorage.setItem(Constants.LS_MediaQueryKey, JSON.stringify(query));
    let res = this.epgService.list(query).map((v, i) => {
      console.log("recup epg ", v);
      this._paginator.value.length = v.total;
      return v.result;
    });
    res.subscribe(x => {
      this.medias.next(x);
    });
    return res;
  }

  disconnect() {}
}
