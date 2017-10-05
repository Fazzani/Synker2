using hfa.WebApi.Models;
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
namespace System.Linq
{
    public static class Extentions
    {
        public static PagedResult<T> GetPaged<T>(this IQueryable<T> query, int page, int pageSize) where T : class
        {
            var result = new PagedResult<T>
            {
                CurrentPage = page,
                PageSize = pageSize,
                RowCount = query.Count()
            };

            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);

            result.Results = query.Skip(page * pageSize).Take(pageSize).ToList();

            return result;
        }

        public static PagedResult<T> GetPaged<T>(this IOrderedQueryable<T> query, int page, int pageSize) where T : class
            => (query as IQueryable<T>).GetPaged(page, pageSize);
    }
}
namespace System
{
    public static class Extensions
    {
        public static string FirstCharToUpper(this string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

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