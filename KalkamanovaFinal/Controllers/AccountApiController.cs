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
using KalkamanovaFinal.Models;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;

namespace KalkamanovaFinal.Controllers
{
    /// <summary>
    /// Контроллер API для управления учетными записями пользователей.
    /// </summary>
    [RoutePrefix("api/Account")]
    public class AccountApiController : ApiController
    {
        /// <summary>
        /// Менеджер для входа пользователя.
        /// </summary>
        private ApplicationSignInManager _signInManager;

        /// <summary>
        /// Менеджер для управления пользователями.
        /// </summary>
        private ApplicationUserManager _userManager;

        /// <summary>
        /// Контекст базы данных.
        /// </summary>
        private ApplicationDbContext _context;

        /// <summary>
        /// Электронная почта пользователя.
        /// </summary>
        private string _userEmail;

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public AccountApiController()
        {
            _context = new ApplicationDbContext();
        }
        
        /// <summary>
        /// Менеджер для входа пользователя.
        /// </summary>
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        /// <summary>
        /// Менеджер для управления пользователями.
        /// </summary>
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        /// <summary>
        /// Регистрация нового пользователя.
        /// </summary>
        [HttpPost]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, UserDomainName = model.UserDomainName };
            var result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            var newUser = new User { Id = Guid.Parse(user.Id), UserDomainName = user.UserDomainName };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

            return Ok(new { Message = "Registration successful", User = user });
        }

        /// <summary>
        /// Обработка результатов ошибки Identity.
        /// </summary>
        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        /// <summary>
        /// Вход пользователя.
        /// </summary>
        [HttpPost]
        [Route("Login")]
        public async Task<IHttpActionResult> Login(LoginModel model)
        {
            var user = await this.UserManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return this.Unauthorized();
            }

            var isPasswordValid = await this.UserManager.CheckPasswordAsync(user, model.Password);

            if (!isPasswordValid)
            {
                return this.Unauthorized();
            }

            var identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Id));

            var properties = new AuthenticationProperties();
            var ticket = new AuthenticationTicket(identity, properties);

            var accessToken = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);

            return this.Ok(new { AccessToken = accessToken });
        }

        /// <summary>
        /// Выход пользователя.
        /// </summary>
        [HttpPost]
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ExternalBearer);
            return this.Ok();
        }
        
        /// <summary>
        /// Создание новой сделки.
        /// </summary>
        [System.Web.Http.Route("CreateTrade")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> CreateTrade(Trade trade)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            trade.CreatedAt = DateTime.Now;

            var userId = User.Identity.GetUserId();
            
            if (userId == null)
            {
                if (Request.Headers.Authorization != null && Request.Headers.Authorization.Scheme == "Bearer")
                {
                    var accessToken = Request.Headers.Authorization.Parameter;
                    var ticket = Startup.OAuthOptions.AccessTokenFormat.Unprotect(accessToken);
                    if (ticket != null)
                    {
                        var claim = ticket.Identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                        if (claim != null)
                        {
                            userId = claim.Value;
                        }
                    }
                }
                else
                {
                    return Unauthorized();
                }
            }

            var appUser = await UserManager.FindByIdAsync(userId);

            if (appUser == null)
            {
                return NotFound();
            }

            var appUserId = Guid.Parse(appUser.Id);
            var user = _context.Users.FirstOrDefault(u => u.Id == appUserId);

            if (user == null)
            {
                return NotFound();
            }

            var data = new Data
            {
                User = user,
                UserId = user.Id,
                Entity = JsonConvert.SerializeObject(trade)
            };

            _context.Data.Add(data);
            await _context.SaveChangesAsync();

            return Ok(trade);
        }
        
        /// <summary>
        /// Получение последней сделки пользователя.
        /// </summary>
        [System.Web.Http.Route("GetLatestTrade")]
        [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> GetLatestTrade()
        {
            var userId = User.Identity.GetUserId();
            
            if (userId == null)
            {
                if (Request.Headers.Authorization != null && Request.Headers.Authorization.Scheme == "Bearer")
                {
                    var accessToken = Request.Headers.Authorization.Parameter;
                    var ticket = Startup.OAuthOptions.AccessTokenFormat.Unprotect(accessToken);
                    if (ticket != null)
                    {
                        var claim = ticket.Identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                        if (claim != null)
                        {
                            userId = claim.Value;
                        }
                    }
                }
                else
                {
                    return Unauthorized();
                }
            }
            
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            var appUser = userManager.FindById(userId);

            if (appUser == null)
            {
                return NotFound();
            }

            var appUserId = Guid.Parse(appUser.Id);
            var userData = _context.Data.Where(d => d.UserId == appUserId).ToList();

            Trade latestTrade = null;

            foreach (var data in userData)
            {
                var trade = JsonConvert.DeserializeObject<Trade>(data.Entity);

                if (latestTrade == null || trade.CreatedAt > latestTrade.CreatedAt)
                {
                    latestTrade = trade;
                }
            }

            if (latestTrade == null)
            {
                return NotFound();
            }

            var result = new TradeResult
            {
                Date = latestTrade.CreatedAt.ToString("dd-MM-yyyy:HH:mm:ss"),
                Amount = latestTrade.Amount
            };

            return Ok(result);
        }

        /// <summary>
        /// Модель для входа пользователя.
        /// </summary>
        public class LoginModel
        {
            /// <summary>
            /// Адрес электронной почты пользователя.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// Пароль пользователя.
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// Флаг для запоминания пользователя.
            /// </summary>
            public bool RememberMe { get; set; }
        }

    }
}