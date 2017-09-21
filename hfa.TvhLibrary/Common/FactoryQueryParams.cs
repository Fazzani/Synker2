using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TvheadendLibrary.QueryParams;

namespace TvheadendLibrary.Common
{
    internal static class FactoryQueryParamsFilter
    {
        public static IQueryParamsFilter CreateStringQueryParamsFilter(string field, string value) =>
            new StringQueryParamsFilter(field, value);

        public static IQueryParamsFilter CreateNumericQueryParamsFilter(string field, string value, string comparaison) => 
            new NumericQueryParamsFilter(field,value, comparaison);
    }
}
