﻿using Nest;
using System.Collections;
using System.Collections.Generic;

namespace Hfa.WebApi.Models
{
    public interface IListResultModel<out T> where T : class
    {
        long Took { get; }
        long Total { get; }
        double MaxScore { get; }

        IEnumerable<T> Result { get; }
    }
}