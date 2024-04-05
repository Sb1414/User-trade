using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System.Security.Claims;
using System.Linq;

namespace KalkamanovaFinal.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountApiController : ApiController
    {
        private readonly UserManager<IdentityUser> userManager;

        public AccountApiController()
        {
            this.userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>());
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(LoginModel model)
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var passwordHash = new PasswordHasher().HashPassword(model.Password);
            user.PasswordHash = passwordHash;

            var result = await this.userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                return this.BadRequest(string.Join(", ", result.Errors));
            }

            return this.Ok();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IHttpActionResult> Login(LoginModel model)
        {
            var user = await this.userManager.FindByEmailAsync(model.Email);
            {
                var users = this.userManager.Users;
                foreach (var usera in users)
                {
                    Console.WriteLine(usera.UserName);
                }
            }

            if (user == null)
            {
                return this.Unauthorized();
            }

            var isPasswordValid = await this.userManager.CheckPasswordAsync(user, model.Password);

            if (!isPasswordValid)
            {
                return this.Unauthorized();
            }

            var identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

            var properties = new AuthenticationProperties();
            var ticket = new AuthenticationTicket(identity, properties);

            var accessToken = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);
            return this.Ok(new { AccessToken = accessToken });
        }

        [HttpPost]
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ExternalBearer);
            return this.Ok();
        }
        
        public class LoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public bool RememberMe { get; set; }
        }
    }
}