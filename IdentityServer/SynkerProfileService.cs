namespace IdentityServer
{
    using IdentityModel;
    using IdentityServer4.Extensions;
    using IdentityServer4.Models;
    using IdentityServer4.Quickstart.UI;
    using IdentityServer4.Services;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class SynkerProfileService : IProfileService
    {
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = TestUsers.Users.FirstOrDefault(x => x.SubjectId == sub);
            if (user != null)
            {
                var claims = user.Claims;
                claims.Add(new Claim(JwtClaimTypes.GivenName, user.Username));
                claims.Add(new Claim("email_hash", user.Username.GetHashCode().ToString()));

                context.IssuedClaims.AddRange(claims);
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = TestUsers.Users.FirstOrDefault(x => x.SubjectId == sub);

            context.IsActive = user?.IsActive == true;
        }
    }
}
