using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models
{
    public class ElasticQueryAggrRequest
    {
        public string Filter { get; set; }
        public int Size { get; set; } = 10000;
    }
}
