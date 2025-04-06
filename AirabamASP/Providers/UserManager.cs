using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AirabamASP.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace AirabamASP.Providers
{
    public interface IUserManager
    {
        Task SignIn(HttpContext httpContext, CookieUserItem user, bool isPersistent = false);
        Task SignOut(HttpContext httpContext);
    }

    public class UserManager : IUserManager
    {
        public async Task SignIn(HttpContext httpContext, CookieUserItem user, bool isPersistent = false)
        {
            string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            var claims = GetUserClaims(user);


            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, authenticationScheme);
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties
            {
            };

            await httpContext.SignInAsync(authenticationScheme, claimsPrincipal, authProperties);
        }

        public async Task SignOut(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private List<Claim> GetUserClaims(CookieUserItem user)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.Name));
            claims.Add(new Claim(ClaimTypes.Email, user.EmailAddress));
            return claims;
        }
    }
}