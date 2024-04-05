using System;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using KalkamanovaFinal.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;

namespace KalkamanovaFinal.Controllers
{
    public class TradeViewController : Controller
    {
        private ApplicationDbContext _context;

        public TradeViewController()
        {
            _context = new ApplicationDbContext();
        }
        public ActionResult CreateTrade()
        {
            return View();
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTrade(Trade trade)
        {
            if (ModelState.IsValid)
            {
                trade.CreatedAt = DateTime.Now;
                
                var userId = User.Identity.GetUserId();
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
                var appUser = userManager.FindById(userId);

                if (appUser == null)
                {
                    // Handle the situation when the user is not found, e.g., return an error view
                    return View("Error", new ErrorViewModel { ErrorMessage = "User not found" });
                }

                // Find the User object that corresponds to the ApplicationUser
                var appUserId = Guid.Parse(appUser.Id);
                var user = _context.Users.FirstOrDefault(u => u.Id == appUserId);

                if (user == null)
                {
                    // Handle the situation when the user is not found, e.g., return an error view
                    return View("Error", new ErrorViewModel { ErrorMessage = "User not found" });
                }

                // Create the Data object
                var data = new Data
                {
                    User = user,
                    UserId = user.Id,
                    Entity = JsonConvert.SerializeObject(trade)
                };

                // Save the Trade and Data objects
                _context.Data.Add(data);
                _context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            return View(trade);
        }
    }
}