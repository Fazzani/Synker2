import { NgModule } from "@angular/core";

export class SimpleQueryElastic {
  From: number;
  Size: number;
  Query: string;
  IndexName: string;
}

/**
 * Elastic Query
 *
 * @export
 * @interface ElasticQuery
 */
export class ElasticQuery {
  from: number;
  size: number;
  query: IElasticSubQuery | Object | null;
  sort: any;

  public static Match(field: string, query: string): IElasticQueryMatch {
    let res = <IElasticSubQuery>{};
    res.query = <IElasticQueryMatch>{};
    res.query.match = <IElasticQueryMatch>{};
    res.query.match[field] = query;
    return res;
  }
}

export interface IElasticSubQuery {
  query: any | null;
}

export interface IElasticQueryMatch {
  query: any | null;
}

export interface IElasticQueryMatchPhrase {
  match_phrase: any | null;
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
  key: string;
  keyAsString: null;
  docCount: number;
  docCountErrorUpperBound: any;
  aggregations: any;
}
