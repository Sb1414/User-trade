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
                    return View("Error", new ErrorViewModel { ErrorMessage = "User not found" });
                }

                var appUserId = Guid.Parse(appUser.Id);
                var user = _context.Users.FirstOrDefault(u => u.Id == appUserId);

                if (user == null)
                {
                    return View("Error", new ErrorViewModel { ErrorMessage = "User not found" });
                }

                var data = new Data
                {
                    User = user,
                    UserId = user.Id,
                    Entity = JsonConvert.SerializeObject(trade)
                };

                _context.Data.Add(data);
                _context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            return View(trade);
        }
        
        // TradeViewController.cs
        [System.Web.Mvc.HttpGet]
        public ActionResult GetLatestTrade()
        {
            var userId = User.Identity.GetUserId();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            var appUser = userManager.FindById(userId);

            if (appUser == null)
            {
                ViewBag.ErrorMessage = "User not found";
                return View("TradeView");
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
                ViewBag.ErrorMessage = "Latest Trade not found";
                return View("TradeView");
            }

            var result = new TradeResult
            {
                Date = latestTrade.CreatedAt.ToString("dd-MM-yyyy:HH:mm:ss"),
                Amount = latestTrade.Amount
            };

            return View(result);
        }
    }
}