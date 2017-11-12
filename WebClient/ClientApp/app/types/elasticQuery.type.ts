import { NgModule } from "@angular/core";

/**
 * Elastic Query
 * 
 * @export
 * @interface ElasticQuery
 */
export interface ElasticQuery {
    from: number;
    size: number;
    query: Object | null;
    sort: any;
}

/**
 * Elastic Response
 * 
 * @export
 * @interface ElasticResponse
 * @template T 
 */
export interface ElasticResponse<T> {
    /**
     * Spended time
     * 
     * @type {number}
     * @memberof ElasticResponse
     */
    took: number;
    /**
     * Score (float)
     * 
     * @type {number}
     * @memberof ElasticResponse
     */
    maxScore: number;
    /**
     * Total matched items
     * 
     * @type {number}
     * @memberof ElasticResponse
     */
    total: number;
    /**
     * Hits
     * 
     * @type {number}
     * @memberof ElasticResponse
     */
    hits: number;
    /**
     * The result
     * 
     * @type {Array<T>}
     * @memberof ElasticResponse
     */
    result: Array<T>;
}

/**
 * Tvg media (channel)
 * 
 * @export
 * @interface TvgMedia
 */
export interface TvgMedia {
    id: string;
    name: string;
    group?: string;
    position: number;
    enabled: boolean;
    mediaType: number;
    url: string;
    lang: string;
    urls: Array<string>;
    tvg?: Tvg;
    isValid: boolean;
    startLineHeader?: string;
}

/**
 * Tvg channel field
 * 
 * @export
 * @interface Tvg
 */
export interface Tvg {
    id: string;
    logo?: string;
    name: string;
    tvgIdentify: string;
}


/**
 * PageResultBase
 */
export interface PagedResultBase {
    CurrentPage: number;
    PageCount: number;
    PageSize: number;
    RowCount: number;
    FirstRowOnPage: number;
    LastRowOnPage: number;
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
    Results: Array<T>;
}

export interface ElasticAggregations {
    key: string,
    keyAsString: null,
    docCount: number,
    docCountErrorUpperBound: any,
    aggregations: any
}