namespace Hfa.WebApi.Models
{
    using System.Collections.Generic;
    public interface IListResultModel<out T> where T : class
    {
        long Took { get; }
        long Total { get; }
        double MaxScore { get; }

        IEnumerable<T> Result { get; }
    }
}