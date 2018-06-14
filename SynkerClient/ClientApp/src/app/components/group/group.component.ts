import { Component, OnInit, Injectable } from "@angular/core";
import { FlatTreeControl } from "@angular/cdk/tree";
import { BehaviorSubject, Observable } from "rxjs";
import { map } from "rxjs/operators";
import { LoadmoreNode, LoadmoreFlatNode } from "../../types/common.type";
import { PlaylistService } from "../../services/playlists/playlist.service";
import { TvgMedia, MediaGroup } from "../../types/media.type";
import { MatTreeFlatDataSource, MatTreeFlattener } from "@angular/material";
import { ActivatedRoute } from "@angular/router";

const LOAD_MORE = "LOAD_MORE";

/**
 * A database that only load part of the data initially. After user clicks on the `Load more`
 * button, more data will be loaded.
 */
@Injectable()
export class LoadmoreDatabase {
  constructor(private playlistService: PlaylistService) {}
  batchNumber = 5;
  dataChange: BehaviorSubject<LoadmoreNode<MediaGroup>[]> = new BehaviorSubject<LoadmoreNode<MediaGroup>[]>([]);
  nodeMap: Map<MediaGroup, LoadmoreNode<MediaGroup>> = new Map<MediaGroup, LoadmoreNode<MediaGroup>>();

  /** The data */
  rootLevelNodes = ["Vegetables", "Fruits"];
  dataMap = new Map([
    ["Fruits", ["Apple", "Orange", "Banana"]],
    ["Vegetables", ["Tomato", "Potato", "Onion"]],
    ["Apple", ["Fuji", "Macintosh"]],
    ["Onion", ["Yellow", "White", "Purple", "Green", "Shallot", "Sweet", "Red", "Leek"]]
  ]);

  initialize(id: string) {
    this.playlistService
      .groups(id)
      .pipe(map(media => media.map(m => new LoadmoreNode<MediaGroup>(m, true))))
      .subscribe(x => this.dataChange.next(x));
  }

  /** Expand a node whose children are not loaded */
  loadMore(item: MediaGroup, onlyFirstTime: boolean = false) {
    if (!this.nodeMap.has(item) || !this.dataMap.has(item.name)) {
      return;
    }
    const parent = this.nodeMap.get(item)!;
    const children = this.dataMap.get(item.name)!;
    if (onlyFirstTime && parent.children!.length > 0) {
      return;
    }
    const newChildrenNumber = parent.children!.length + this.batchNumber;
    let nodes = children.slice(0, newChildrenNumber).map(name => this._generateNode(<MediaGroup>{}));
    if (newChildrenNumber < children.length) {
      // Need a new load more node
      nodes.push(new LoadmoreNode(item, false, item.name));
    }

    parent.childrenChange.next(nodes);
    this.dataChange.next(this.dataChange.value);
  }

  private _generateNode(item: MediaGroup): LoadmoreNode<MediaGroup> {
    if (this.nodeMap.has(item)) {
      return this.nodeMap.get(item)!;
    }
    const result = new LoadmoreNode(item, this.dataMap.has(item.name));
    this.nodeMap.set(item, result);
    return result;
  }
}

@Component({
  selector: "app-group",
  templateUrl: "./group.component.html",
  styleUrls: ["./group.component.scss"],
  providers: [LoadmoreDatabase]
})
export class GroupComponent implements OnInit {

  playlistId: string;
  nodeMap: Map<string, LoadmoreFlatNode<MediaGroup>> = new Map<string, LoadmoreFlatNode<MediaGroup>>();
  treeControl: FlatTreeControl<LoadmoreFlatNode<MediaGroup>>;
  treeFlattener: MatTreeFlattener<LoadmoreNode<MediaGroup>, LoadmoreFlatNode<MediaGroup>>;
  // Flat tree data source
  dataSource: MatTreeFlatDataSource<LoadmoreNode<MediaGroup>, LoadmoreFlatNode<MediaGroup>>;
  routeSub: any;

  constructor(private route: ActivatedRoute, private database: LoadmoreDatabase) {
    this.treeFlattener = new MatTreeFlattener(this.transformer, this.getLevel, this.isExpandable, this.getChildren);
    this.treeControl = new FlatTreeControl<LoadmoreFlatNode<MediaGroup>>(this.getLevel, this.isExpandable);
    this.dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

    database.dataChange.subscribe(data => {
      this.dataSource.data = data;
    });

    this.routeSub = this.route.params.subscribe(params => {
      this.playlistId = params["id"]; // (+) converts string 'id' to a number
      database.initialize(this.playlistId);
    });
  }

  ngOnInit(): void {
    //  throw new Error("Method not implemented.");
  }

  getChildren = (node: LoadmoreNode<MediaGroup>): Observable<LoadmoreNode<MediaGroup>[]> => {
    return node.childrenChange;
  };

  transformer = (node: LoadmoreNode<MediaGroup>, level: number) => {
    if (this.nodeMap.has(node.item.name)) {
      return this.nodeMap.get(node.item.name)!;
    }
    let newNode = new LoadmoreFlatNode(node.item, level, node.hasChildren, node.loadMoreParentItem);
    this.nodeMap.set(node.item.name, newNode);
    return newNode;
  };

  getLevel = (node: LoadmoreFlatNode<MediaGroup>) => {
    return node.level;
  };

  isExpandable = (node: LoadmoreFlatNode<MediaGroup>) => {
    return node.expandable;
  };

  hasChild = (_: number, _nodeData: LoadmoreFlatNode<MediaGroup>) => {
    return _nodeData.expandable;
  };

  isLoadMore = (_: number, _nodeData: LoadmoreFlatNode<MediaGroup>) => {
    return _nodeData.item === null;
  };

  /** Load more nodes from data source */
  loadMore(item: MediaGroup) {
    this.database.loadMore(item);
  }

  loadChildren(node: LoadmoreFlatNode<MediaGroup>) {
    this.database.loadMore(node.item, true);
  }
}
