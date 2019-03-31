namespace hfa.WebApi.Common.Auth
{
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    public class AuthorizePolicies
    {
        public const string ADMIN = "administrators";
        public const string READER = "reader";
        public const string FULLACCESS = "fullaccess";

        public static string READER_ONLY = "readerOnly";
    }

    public class Authentication
    {
        public const string AuthSchemes =
        CookieAuthenticationDefaults.AuthenticationScheme + "," +
        JwtBearerDefaults.AuthenticationScheme;
    }
}
