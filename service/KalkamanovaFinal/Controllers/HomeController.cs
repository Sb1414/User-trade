using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KalkamanovaFinal.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// Контроллер для домашней страницы.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Отображает домашнюю страницу.
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }
    }
}