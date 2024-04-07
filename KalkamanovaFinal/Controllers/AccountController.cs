using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using KalkamanovaFinal.Models;
using Newtonsoft.Json;

namespace KalkamanovaFinal.Controllers
{
    /// <summary>
    /// Контроллер управления учетными записями пользователей.
    /// </summary>
    [System.Web.Mvc.Authorize]
    public class AccountController : Controller
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
        /// Конструктор по умолчанию.
        /// </summary>
        public AccountController()
        {
            _context = new ApplicationDbContext();
        }
        
        /// <summary>
        /// Конструктор с параметрами.
        /// </summary>
        /// <param name="userManager">Менеджер пользователей.</param>
        /// <param name="signInManager">Менеджер для входа.</param>
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        /// <summary>
        /// Менеджер для входа пользователя.
        /// </summary>
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
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
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        
        // GET: /Account/GetUsers
        /// <summary>
        /// Получение списка пользователей.
        /// </summary>
        [System.Web.Mvc.HttpGet]
        public ContentResult GetUsers()
        {
            var users = _context.Users.ToList();
            return Content(JsonConvert.SerializeObject(users), "application/json");
        }

        /// <summary>
        /// Получение всех пользователей.
        /// </summary>
        [System.Web.Mvc.HttpGet]
        public JsonResult GetAllUsers()
        {
            var users = UserManager.Users.ToList();
            
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        // GET: /Account/Login
        /// <summary>
        /// Отображает страницу входа.
        /// </summary>
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        /// <summary>
        /// Обрабатывает запрос на вход пользователя.
        /// </summary>
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe,
                shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        // GET: /Account/Register
        /// <summary>
        /// Отображает страницу регистрации.
        /// </summary>
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        /// <summary>
        /// Обрабатывает запрос на регистрацию пользователя.
        /// </summary>
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.AllowAnonymous]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, UserDomainName = model.UserDomainName };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var newUser = new User { Id = Guid.Parse(user.Id), UserDomainName = user.UserDomainName };
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            return View(model);
        }

        // GET: /Account/ConfirmEmail
        /// <summary>
        /// Подтверждение адреса электронной почты.
        /// </summary>
        [System.Web.Mvc.AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        // POST: /Account/LogOff
        /// <summary>
        /// Выход из системы.
        /// </summary>
        [System.Web.Mvc.HttpPost]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/ExternalLoginFailure
        /// <summary>
        /// Отображает страницу ошибки внешней аутентификации.
        /// </summary>
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        /// <summary>
        /// Освобождает ресурсы, используемые контроллером.
        /// </summary>
        /// <param name="disposing">True, чтобы освободить управляемые ресурсы.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        private const string XsrfKey = "XsrfId";

        /// <summary>
        /// Менеджер аутентификации.
        /// </summary>
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        /// <summary>
        /// Добавляет ошибки к модели.
        /// </summary>
        /// <param name="result">Результат операции.</param>
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        /// <summary>
        /// Перенаправляет на локальную страницу, если такая есть.
        /// </summary>
        /// <param name="returnUrl">URL для возврата.</param>
        /// <returns>Результат перенаправления.</returns>
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}