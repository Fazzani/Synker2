"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var rxjs_1 = require("rxjs");
var QueryListBaseModel = /** @class */ (function () {
    function QueryListBaseModel() {
    }
    return QueryListBaseModel;
}());
exports.QueryListBaseModel = QueryListBaseModel;
var SortDirectionEnum;
(function (SortDirectionEnum) {
    SortDirectionEnum[SortDirectionEnum["asc"] = 1] = "asc";
    SortDirectionEnum[SortDirectionEnum["desc"] = 2] = "desc";
})(SortDirectionEnum = exports.SortDirectionEnum || (exports.SortDirectionEnum = {}));
var KEY_CODE;
(function (KEY_CODE) {
    KEY_CODE[KEY_CODE["A"] = 1] = "A";
    KEY_CODE[KEY_CODE["B"] = 2] = "B";
    KEY_CODE[KEY_CODE["C"] = 3] = "C";
    KEY_CODE[KEY_CODE["D"] = 4] = "D";
    KEY_CODE[KEY_CODE["E"] = 5] = "E";
    KEY_CODE[KEY_CODE["F"] = 6] = "F";
    KEY_CODE[KEY_CODE["RIGHT_ARROW"] = 39] = "RIGHT_ARROW";
    KEY_CODE[KEY_CODE["LEFT_ARROW"] = 37] = "LEFT_ARROW";
})(KEY_CODE = exports.KEY_CODE || (exports.KEY_CODE = {}));
var KEY;
(function (KEY) {
    KEY["A"] = "a";
    KEY["B"] = "b";
    KEY["C"] = "c";
    KEY["D"] = "d";
    KEY["E"] = "e";
    KEY["F"] = "f";
    KEY["G"] = "d";
    KEY["H"] = "h";
    KEY["I"] = "i";
    KEY["J"] = "j";
    KEY["K"] = "k";
    KEY["L"] = "l";
    KEY["M"] = "m";
    KEY["N"] = "n";
    KEY["O"] = "o";
    KEY["P"] = "p";
    KEY["Q"] = "q";
    KEY["R"] = "r";
    KEY["S"] = "s";
    KEY["T"] = "t";
})(KEY = exports.KEY || (exports.KEY = {}));
var Exception = /** @class */ (function () {
    function Exception() {
    }
    return Exception;
}());
exports.Exception = Exception;
/** Nested node */
var LoadmoreNode = /** @class */ (function () {
    function LoadmoreNode(item, hasChildren, loadMoreParentItem) {
        if (hasChildren === void 0) { hasChildren = false; }
        if (loadMoreParentItem === void 0) { loadMoreParentItem = null; }
        this.item = item;
        this.hasChildren = hasChildren;
        this.loadMoreParentItem = loadMoreParentItem;
        this.childrenChange = new rxjs_1.BehaviorSubject([]);
    }
    Object.defineProperty(LoadmoreNode.prototype, "children", {
        get: function () {
            return this.childrenChange.value;
        },
        enumerable: true,
        configurable: true
    });
    return LoadmoreNode;
}());
exports.LoadmoreNode = LoadmoreNode;
/** Flat node with expandable and level information */
var LoadmoreFlatNode = /** @class */ (function () {
    function LoadmoreFlatNode(item, level, expandable, loadMoreParentItem) {
        if (level === void 0) { level = 1; }
        if (expandable === void 0) { expandable = false; }
        if (loadMoreParentItem === void 0) { loadMoreParentItem = null; }
        this.item = item;
        this.level = level;
        this.expandable = expandable;
        this.loadMoreParentItem = loadMoreParentItem;
    }
    return LoadmoreFlatNode;
}());
exports.LoadmoreFlatNode = LoadmoreFlatNode;
//# sourceMappingURL=common.type.js.map