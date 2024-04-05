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
    public class TradeController : Controller
    {
        private ApplicationDbContext _context;

        public TradeController()
        {
            _context = new ApplicationDbContext();
        }
        
        [System.Web.Http.HttpGet]
        public JsonResult GetLatestTrade()
        {
            var userId = User.Identity.GetUserId();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            var appUser = userManager.FindById(userId);

            if (appUser == null)
            {
                return Json("User not found");
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
                return Json("User not found");
            }

            var result = new TradeResult
            {
                Date = latestTrade.CreatedAt.ToString("dd-MM-yyyy:HH:mm:ss"),
                Amount = latestTrade.Amount
            };

            return Json(result, JsonRequestBehavior.AllowGet);

        }
    }
    
    public class TradeResult
    {
        public string Date { get; set; }
        public decimal Amount { get; set; }
    }
}