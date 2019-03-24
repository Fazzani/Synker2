namespace hfa.WebApi.Common.Auth
{
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    public class AuthorizePolicies
    {
        public const string ADMIN = "Administrators";
        public const string READER = "Reader";
        public const string FULLACCESS = "fullaccess";
    }

    public class Authentication
    {
        public const string AuthSchemes =
        CookieAuthenticationDefaults.AuthenticationScheme + "," +
        JwtBearerDefaults.AuthenticationScheme;
    }
}
