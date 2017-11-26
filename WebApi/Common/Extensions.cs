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
        public static IListResultModel<T2> GetResultListModel<T, T2>(this Nest.ISearchResponse<T> searchResponse) where T :  class where T2: class, IModel<T, T2>, new()
        => new ListResultModel<T, T2>(searchResponse);
    }
}

namespace System.Linq
{
    public static class Extentions
    {
        public static PagedResult<T> GetPaged<T>(this IEnumerable<T> query, int page, int pageSize) where T : class
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
            => (query as IEnumerable<T>).GetPaged(page, pageSize);

        public static PagedResult<T> GetPaged<T>(this IQueryable<T> query, int page, int pageSize) where T : class
            => (query as IEnumerable<T>).GetPaged(page, pageSize);
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


namespace NETCore.Encrypt.Extensions.Internal
{
    using System.Security.Cryptography;
    using System.Xml;

    public static class RsaExtension
    {

        public static void FromXmlString(this RSA rsa, string xmlString, bool ex)
        {
            if (string.IsNullOrEmpty(xmlString))
                throw new ArgumentNullException(nameof(xmlString));

            var parameters = new RSAParameters();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Modulus": parameters.Modulus = Convert.FromBase64String(node.InnerText); break;
                        case "Exponent": parameters.Exponent = Convert.FromBase64String(node.InnerText); break;
                        case "P": parameters.P = Convert.FromBase64String(node.InnerText); break;
                        case "Q": parameters.Q = Convert.FromBase64String(node.InnerText); break;
                        case "DP": parameters.DP = Convert.FromBase64String(node.InnerText); break;
                        case "DQ": parameters.DQ = Convert.FromBase64String(node.InnerText); break;
                        case "InverseQ": parameters.InverseQ = Convert.FromBase64String(node.InnerText); break;
                        case "D": parameters.D = Convert.FromBase64String(node.InnerText); break;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid XML RSA key.");
            }

            rsa.ImportParameters(parameters);
        }
    }
}