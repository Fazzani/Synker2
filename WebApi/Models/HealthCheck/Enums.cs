using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models.HealthCheck
{
    public enum HealthCheckEnum : byte
    {
        WebApi = 0,
        Database = 1,
        Elastic = 2
    }
}
