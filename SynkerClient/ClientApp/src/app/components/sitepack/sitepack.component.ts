import { of, fromEvent, BehaviorSubject, Subscription, Observable } from 'rxjs';
import { debounceTime, switchMap, distinctUntilChanged, map, filter, tap, merge } from 'rxjs/operators';
import { Component, OnInit, ViewChild, ElementRef, OnDestroy, Inject } from "@angular/core";
import { DataSource } from "@angular/cdk/collections";
import { MatPaginator, PageEvent, MatSort, MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatSnackBar } from "@angular/material";
import { ENTER, COMMA } from "@angular/cdk/keycodes";
import { MatChipInputEvent } from "@angular/material";
import { CommonService, Constants } from "../../services/common/common.service";
import { picon } from "../../types/picon.type";
import { snakbar_duration } from "../../variables";
import { PiconService } from "../../services/picons/picons.service";
import { SitePackService } from "../../services/sitepack/sitepack.service";
import { sitePackChannel } from "../../types/sitepackchannel.type";
import { SimpleQueryElastic } from "../../types/elasticQuery.type";
import { BreakpointObserver, BreakpointState } from '@angular/cdk/layout';

@Component({
  selector: "sitepack",
  templateUrl: "./sitepack.component.html",
  providers: [SitePackService]
})
/** mediaref component*/
export class SitePackComponent implements OnInit, OnDestroy {
  subscriptionTableEvent: Subscription;
  displayedColumns : Array<string>;
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;
  @ViewChild('filter') filter: ElementRef;
  dataSource: SitePackDataSource | null;
  currentItem: sitePackChannel | null;

  /** media ctor */
  constructor(
    private sitePackService: SitePackService,
    private piconService: PiconService,
    private commonService: CommonService,
    public dialog: MatDialog,
    public snackBar: MatSnackBar,
    breakpointObserver: BreakpointObserver
  ) {
    breakpointObserver.observe(['(max-width: 600px)']).subscribe(result => {
      this.displayedColumns = result.matches ?
        ["displayNames", 'country', 'channel_name','site_id', 'actions']:
        ["logo", "update", "displayNames", "site", 'country', 'mediaType', 'channel_name', 'site_id', 'actions'];
    });
  }

  /** Called by Angular after media component initialized */
  ngOnInit(): void {
    let storedQuery = this.commonService.JsonToObject<SimpleQueryElastic>(localStorage.getItem(Constants.LS_SiteQueryKey));
    console.log('storedQuery ', storedQuery);

    this.paginator.pageIndex = storedQuery != null ? Math.floor(storedQuery.From / storedQuery.Size) : 0;
    this.paginator.pageSizeOptions = [50, 100, 250, 1000];
    this.paginator.pageSize = storedQuery != null ? storedQuery.Size : this.paginator.pageSizeOptions[0];
    this.dataSource = new SitePackDataSource(this.sitePackService, this.paginator);
    this.dataSource.filter = this.filter.nativeElement.value = storedQuery != null ? storedQuery.Query : '';

    this.subscriptionTableEvent = this.paginator.page
      .asObservable().pipe(
        merge(fromEvent<KeyboardEvent>(this.filter.nativeElement, 'keyup')),
        debounceTime(1500),
        distinctUntilChanged())
      .subscribe(x => {
        if (!this.dataSource) {
          return;
        }
        console.log('subscriptionTableEvent => ', x);
        if ((x as PageEvent).pageIndex !== undefined) this.paginator.pageIndex = (x as PageEvent).pageIndex;
        else if ((x as PageEvent).length === undefined) this.paginator.pageIndex = 0;

        this.dataSource.filter = this.filter.nativeElement.value;
        this.dataSource.paginator = this.paginator;
      });
  }

  /**
   * Modify sitePack dialog
   * @param spChannel
   */
  openDialog(spChannel: sitePackChannel): void {
    let dialogRef = this.dialog.open(SitePackModifyDialog, {
      width: '550px',
      data: spChannel
    });

    dialogRef.afterClosed().subscribe(result => {
      this.snackBar.open(spChannel.displayNames[0] + ' was modified', '', {
        duration: snakbar_duration
      });
    });
  }

  /**
   * Synchronize all picons from github
   */
  synkPiconsGlobal(): void {
    this.piconService.synk().subscribe(res => {
      this.snackBar.open('Picons index was synchronized');
    });
  }

  /**
   * Synchronize country field
   */
  synkWebgrab(): void {
    this.sitePackService.synkWebgrab().subscribe(res => {
      this.snackBar.open('Sitepack webgrab config was synchronized');
      this.reload();
    });
  }

  /**
   * Synchronize country field
   */
  synkCountries(): void {
    this.sitePackService.syncCountries().subscribe(res => {
      this.snackBar.open('Country field was synchronized');
      this.reload();
    });
  }

  delete(id: string): void {
    const confirm = window.confirm('Do you really want to delete this media ref?');

    of(id).pipe(
      filter(() => confirm),
      switchMap(x => this.sitePackService.delete(x)))
      .subscribe(res => {
        this.snackBar.open('Medias referentiel was deleted');
        this.dataSource.getData();
      });
  }

  save(): void {
    this.dataSource.save().subscribe(res => {
      this.snackBar.open('site pack was saved successfully');
    });
  }

  /**
   * Force save all site packs
   */
  saveAll(): void {
    this.dataSource.saveAll();
  }

  update(spChannel: sitePackChannel): void {
    this.sitePackService.save(spChannel).subscribe(x => this.snackBar.open('Site pack was synchronized'));
  }

  reload(): void {
    this.ngOnInit();
  }

  /**
   *
   * @param {sitePackChannel} media
   * @param {any} event
   */
  toggleSelected(media: sitePackChannel, event: any): void {
    if (!event.ctrlKey) {
      this.dataSource.data.filter((v, i) => v.id !== media.id).forEach((m, i) => {
        m.selected = false;
      });
    }
    media.selected = !media.selected;
  }

  updateManualPage(index) {
    this.paginator.page.emit(<PageEvent>{ pageIndex: index });
  }

  ngOnDestroy() {
    this.subscriptionTableEvent.unsubscribe();
  }
}

// ---------------------------------------------------------------------------------   Site Pack ModifyDialog
@Component({
  selector: 'sitepack-modify-dialog',
  templateUrl: './sitepack.dialog.html'
})
export class SitePackModifyDialog implements OnInit, OnDestroy {
  @ViewChild('filterPicon') filterPicon: ElementRef;
  piconsFilter: Observable<picon[]>;
  currrentPiconUrl: string;
  visible = true;
  selectable = true;
  removable = true;
  addOnBlur = true;

  // Enter, comma
  separatorKeysCodes = [ENTER, COMMA];
  constructor(
    private piconService: PiconService,
    public dialogRef: MatDialogRef<SitePackModifyDialog>,
    @Inject(MAT_DIALOG_DATA) public data: sitePackChannel
  ) { }

  ngOnInit(): void {
    fromEvent<KeyboardEvent>(this.filterPicon.nativeElement, 'keyup').pipe(
      debounceTime(1000),
      distinctUntilChanged())
      .subscribe((x: KeyboardEvent) => {
        const query = <SimpleQueryElastic>{
          From: 0,
          IndexName: 'picons',
          Query: `name: ${this.filterPicon.nativeElement.value}`,
          Size: 10
        };
        this.piconsFilter = this.piconService
          .search(query).pipe(
            map(x => x.result),
            tap(x => console.log(x)));
      });
  }

  onNoClick(): void {
    this.dialogRef.close();
  }

  onfocusPicon(piconUrl: string): void {
    this.currrentPiconUrl = piconUrl;
    this.filterPicon.nativeElement.value = '';
  }

  onblurPicon(piconPath: string): void {
    if (piconPath === '') {
      this.filterPicon.nativeElement.value = this.currrentPiconUrl;
    }
  }

  add(event: MatChipInputEvent, model): void {
    const input = event.input;
    const value = event.value;
    if ((value || '').trim()) {
      model.push(value.trim());
    }

    // Reset the input value
    if (input) {
      input.value = '';
    }
  }

  remove(data: any, model: any[]): void {
    const index = model.indexOf(data);

    if (index >= 0) {
      model.splice(index, 1);
    }
  }

  ngOnDestroy(): void { }
}

// ----------------------------------------  MediaRef DataSource
/**
 * Media datasource for mat-table component
 */
export class SitePackDataSource extends DataSource<sitePackChannel> {
  data: sitePackChannel[];
  medias = new BehaviorSubject<sitePackChannel[]>([]);
  _filterTvgSiteChange = new BehaviorSubject<string>('');

  _filterChange = new BehaviorSubject<string>('');
  get filter(): string {
    return this._filterChange.value;
  }
  set filter(filter: string) {
    this._filterChange.next(filter);
  }

  _paginator = new BehaviorSubject<MatPaginator>(<MatPaginator>{});
  get paginator(): MatPaginator {
    return this._paginator.value;
  }
  set paginator(paginator: MatPaginator) {
    this._paginator.next(paginator);
  }

  constructor(private sitePackService: SitePackService, private mdPaginator: MatPaginator) {
    super();

    this.paginator = this.mdPaginator;
    this._filterChange.pipe(
      merge(this._paginator),
      debounceTime(300))
      // .distinctUntilChanged()
      .subscribe(x => this.getData());
  }

  connect(): Observable<sitePackChannel[]> {
    return this.medias.asObservable();
  }

  /**
   * Get sitepacks from web api
   * @returns Obersvable<sitePackChannel[]>
   */
  getData(): Observable<sitePackChannel[]> {
    let pageSize = this.paginator.pageSize === undefined ? 25 : this.paginator.pageSize;
    let query = <SimpleQueryElastic>{
      From: pageSize * (isNaN(this.paginator.pageIndex) ? 0 : this.paginator.pageIndex),
      IndexName: 'sitepack',
      Query: this.filter,
      Size: pageSize
    };

    localStorage.setItem(Constants.LS_SiteQueryKey, JSON.stringify(query));

    const res = this.sitePackService.search<sitePackChannel>(query).pipe(map((v, i) => {
      this._paginator.value.length = v.total;
      return v.result;
    }));

    res.subscribe(x => {
      this.medias.next(x);
      this.data = x;
    });

    return res;
  }

  /**
   * Force save all site packs
   */
  saveAll(): void {
    const query = <SimpleQueryElastic>{
      From: 0,
      IndexName: 'sitepack',
      Query: '',
      Size: 1000000
    };

    this.sitePackService
      .search<sitePackChannel>(query).pipe(
        map(v => {
          this.sitePackService.save(...v.result).subscribe();
        }))
      .subscribe(x => console.log(x));
  }

  delete(id: string): void {
    this.sitePackService.delete(id).subscribe((res: number) => {
      let idx = this.medias.value.findIndex(m => m.id === id);
      this.medias.value.splice(idx, 1);
      this.medias.next(this.medias.value);
    });
  }

  save(): any {
    return this.sitePackService.save(...this.medias.value);
  }

  disconnect() { }
}
