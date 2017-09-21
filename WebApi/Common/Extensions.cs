using Hfa.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hfa.WebApi.Common
{
    public static class Extensions
    {
        public static IListResultModel<T> GetResultListModel<T>(this Nest.ISearchResponse<T> searchResponse) where T : class
        => new ListResultModel<T>(searchResponse);
    }
}
