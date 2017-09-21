using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hfa.WebApi.Models
{
    public class ListResultModel<T> : IListResultModel<T> where T : class
    {
        private double _maxScore;
        private long _took;
        private long _hits;
        private long _total;
        private IEnumerable<T> _result;

        public ListResultModel(Nest.ISearchResponse<T> searchResponse)
        {
            _hits = searchResponse.Hits.Count;
            _took = searchResponse.Took;
            _total = searchResponse.Total;
            _result = searchResponse.Documents;
            _maxScore = searchResponse.MaxScore;
        }

        public long Took => _took;
        public double MaxScore => _maxScore;

        public long Total => _total;
        public long Hits => _hits;

        public IEnumerable<T> Result => _result;
    }
}
