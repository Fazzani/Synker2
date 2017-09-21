using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PlaylistBaseLibrary.Common
{
   public class Hash
    {
        public string ComputeHash(string input) =>
                   Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input)));

        public bool VerifHash(string input, string hashValue) => hashValue.Equals(ComputeHash(input));
    }
}
