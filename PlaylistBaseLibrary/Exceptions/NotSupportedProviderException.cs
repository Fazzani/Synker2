using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.PlaylistBaseLibrary.Exceptions
{
    public class NotSupportedProviderException : Exception
    {
        public NotSupportedProviderException(string message) : base(message)
        {

        }

        public NotSupportedProviderException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
