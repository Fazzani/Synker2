using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.PlaylistBaseLibrary.Exceptions
{
    public class NotFoundProviderException : Exception
    {
        public NotFoundProviderException(string message) : base(message)
        {

        }

        public NotFoundProviderException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
