export class QueryListBaseModel {
    public searchDict: Map<string>;
    public sortDict: Map<SortDirectionEnum>;
    public pageNumber: number;
    public pageSize: number;
    public getAll: boolean;
}

interface Map<T> {
    [K: string]: T;
}

export enum SortDirectionEnum {
    asc = 1,
    desc = 2
}

/**
 * PageResultBase
 */
export interface PagedResultBase {
    currentPage: number;
    pageCount: number;
    pageSize: number;
    rowCount: number;
    firstRowOnPage: number;
    lastRowOnPage: number;
}

export interface PageListState {
    pageIndex: number;
    pageSize: number;
    filter: string | any | null;
}

/**
 * PageResult
 * 
 * @export
 * @interface PagedResult
 * @extends {PagedResultBase}
 * @template T 
 */
export interface PagedResult<T> extends PagedResultBase {
    results: Array<T>;
}

export interface ISelectable {
    selected: boolean;
}

export enum KEY_CODE {
    A = 1,
    B = 2,
    C = 3,
    D = 4,
    E = 5,
    F = 6,
    RIGHT_ARROW = 39,
    LEFT_ARROW = 37
}

export enum KEY {
    A = 'a',
    B = 'b',
    C = 'c',
    D = 'd',
    E = 'e',
    F = 'f',
    G = 'd',
    H = 'h',
    I = 'i',
    J = 'j',
    K = 'k',
    L = 'l',
    M = 'm',
    N = 'n',
    O = 'o',
    P = 'p',
    Q = 'q',
    R = 'r',
    S = 's',
    T = 't'
}

export class Exception {
    title: string;
    message: string;
}
