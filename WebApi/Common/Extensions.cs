using hfa.WebApi;
using hfa.WebApi.Models;
using Hfa.WebApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;
using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hfa.WebApi.Common
{
    public static class Extensions
    {
        public static IListResultModel<T2> GetResultListModel<T, T2>(this Nest.ISearchResponse<T> searchResponse) where T : class where T2 : class, IModel<T, T2>, new()
        => new ListResultModel<T, T2>(searchResponse);
        public static void AssertElasticResponse(this IResponse response)
        {
            //Debug.Assert(response.IsValid);
            if (!response.IsValid)
            {
                Common.Logger("Elastic").LogError(response.DebugInformation);
            }
        }

        public class Common
        {
            public static ILogger Logger(string cat = "default") => _LoggerFactory.CreateLogger(cat);
            private static ILoggerFactory _LoggerFactory = null;

            static Common()
            {
                _LoggerFactory = (ILoggerFactory)Startup.Provider.GetService(typeof(ILoggerFactory));
            }

            internal static void DisplayList(Playlist<TvgMedia> pl, Action<string> logAction, string message)
            {
                logAction?.Invoke(message);
                Logger().LogInformation(message);
                if (pl != null)
                {
                    Logger().LogInformation(message);
                }
                //  logAction?.Invoke(pl.ToString(false));
            }
        }



    }
}

namespace System.Linq
{
    public static class Extentions
    {
        public static PagedResult<T> GetPaged<T>(this IEnumerable<T> query, int page, int pageSize, bool getAll) where T : class
        {
            PagedResult<T> result = new PagedResult<T>
            {
                CurrentPage = page,
                PageSize = pageSize,
                RowCount = query.Count()
            };

            double pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);

            result.Results = getAll ? query.ToList() : query.Skip(page * pageSize).Take(pageSize).ToList();

            return result;
        }

        public static PagedResult<T> GetPaged<T>(this IOrderedQueryable<T> query, int page, int pageSize, bool getAll = false) where T : class
            => (query as IEnumerable<T>).GetPaged(page, pageSize, getAll);

        public static PagedResult<T> GetPaged<T>(this IQueryable<T> query, int page, int pageSize, bool getAll = false) where T : class
            => (query as IEnumerable<T>).GetPaged(page, pageSize, getAll);
    }
}
namespace System
{
    public static class Extensions
    {
        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("ARGH!");
            }

            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        public static string HashPassword(this string password, string salt)
        {
            byte[] bytes = new UTF8Encoding().GetBytes(salt + password);
            byte[] hashBytes;
            using (Security.Cryptography.SHA512Managed algorithm = new Security.Cryptography.SHA512Managed())
            {
                hashBytes = algorithm.ComputeHash(bytes);
            }
            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(this string password, string password64, string salt)
        {
            string expectedPassword = HashPassword(password, salt);
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
            {
                throw new ArgumentNullException(nameof(xmlString));
            }

            RSAParameters parameters = new RSAParameters();
            XmlDocument xmlDoc = new XmlDocument();
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

namespace hfa.WebApi.Http
{
    public static class HttpContextExtensions
    {
        public static IApplicationBuilder UseHttpContext(this IApplicationBuilder app)
        {
            SynkerHttpContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>);
            return app;
        }
    }

    public class SynkerHttpContext
    {
        private static Func<IHttpContextAccessor> _httpContextAccessor;

        public static HttpContext Current => _httpContextAccessor().HttpContext;

        public static string AppBaseUrl => Current == null ? "" : $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";

        internal static void Configure(Func<IHttpContextAccessor> contextAccessor)
        {
            _httpContextAccessor = contextAccessor;
        }
    }

}