using System;
using System.Threading.Tasks;
using KalkamanovaFinal.Models;
using Microsoft.AspNet.Identity;

namespace KalkamanovaFinal.Service
{
    public class UserService
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationUserManager userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<(IdentityResult, ApplicationUser)> RegisterUserAsync(RegisterViewModel model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email, UserDomainName = model.UserDomainName };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var newUser = new User { Id = Guid.Parse(user.Id), UserDomainName = user.UserDomainName };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
            }

            return (result, user);
        }
    }
}