export class QueryListBaseModel {
    public searchDict: Map<string>;
    public sortDict: Map<SortDirectionEnum>;
    public pageNumber: number;
    public pageSize: number;
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