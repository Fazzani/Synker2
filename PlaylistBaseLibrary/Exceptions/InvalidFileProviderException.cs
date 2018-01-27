using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.PlaylistBaseLibrary.Exceptions
{
    public class InvalidFileProviderException : Exception
    {
        public InvalidFileProviderException(string message) : base(message)
        {

        }

        public InvalidFileProviderException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
