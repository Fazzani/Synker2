using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models.Elastic
{
    public class SimpleQueryElastic
    {
        public int From { get; set; }
        public int Size { get; set; }
        public string Query { get; set; }
        public string IndexName { get; set; }
    }
}
