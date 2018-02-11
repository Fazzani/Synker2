using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Common.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException()
        { }

        public BusinessException(string message)
        : base(message)
        { }

        public BusinessException(string message, Exception innerException)
        : base(message, innerException)
        { }
    }
}
