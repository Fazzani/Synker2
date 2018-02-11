using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.Synker.batch.EmailModels
{
    public class EmailViewModel
    {
        public string CompanyName { get; set; }
        public string ExternalUrl { get; set; }
        public string ProductName { get; set; }
        
        public DateTime TimeStamp { get; set; }
    }
}
