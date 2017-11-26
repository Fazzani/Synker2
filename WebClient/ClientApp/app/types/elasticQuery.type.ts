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


export interface ElasticAggregations {
    key: string,
    keyAsString: null,
    docCount: number,
    docCountErrorUpperBound: any,
    aggregations: any
}