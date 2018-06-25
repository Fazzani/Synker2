namespace Hfa.WebApi.Models
{
    using Nest;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public class ListResultModel<T, T2> : IListResultModel<T2> where T : class where T2 : class, IModel<T, T2>, new()
    {
        private double _maxScore;
        private long _took;
        private long _total;
        private IEnumerable<T2> _result;

        public ListResultModel(ISearchResponse<T> searchResponse)
        {
            _result = searchResponse.Hits.Select(x => new T2().ToModel(x));

            _took = searchResponse.Took;
            _total = searchResponse.Total;
            _maxScore = searchResponse.MaxScore;
        }

        public long Took => _took;

        public double MaxScore => _maxScore;

        public long Total => _total;

        public IEnumerable<T2> Result => _result;
    }

    public interface IModel<T, out T2> where T2 : new() where T : class
    {
        T2 ToModel(IHit<T> hit);
    }



}
