using Hfa.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hfa.WebApi.Common
{
    public static class Extensions
    {
        public static IListResultModel<T> GetResultListModel<T>(this Nest.ISearchResponse<T> searchResponse) where T : class
        => new ListResultModel<T>(searchResponse);
    }


}

namespace System
{
    public static class Extensions
    {
        public static string HashPassword(this string password, string salt)
        {
            var bytes = new UTF8Encoding().GetBytes(salt + password);
            byte[] hashBytes;
            using (var algorithm = new Security.Cryptography.SHA512Managed())
            {
                hashBytes = algorithm.ComputeHash(bytes);
            }
            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(this string password, string password64, string salt)
        {
            var expectedPassword = HashPassword(password, salt);
            return expectedPassword.Equals(password64);
        }
    }
}