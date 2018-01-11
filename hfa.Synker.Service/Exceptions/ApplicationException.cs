using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.Synker.Service.Exceptions
{
    public class ApplicationException : Exception
    {
        public ApplicationException()
        {

        }

        public ApplicationException(string message):base(message)
        {

        }
    }
}
