using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTechnology_Final.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using MimeKit;
using Microsoft.EntityFrameworkCore;
using NetTechnology_Final.Context;

namespace NetTechnology_Final.Controllers
{
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [OnlyUnauthenticated]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [OnlyUnauthenticated]
        public async Task<IActionResult> Login(Accounts account)
        {
            if (ModelState.IsValid)
            {
                return View();
            }
            var adminAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.username == account.username && a.password == account.password);

            if (adminAccount != null)
            {
                await saveLogin(adminAccount);
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }

        private async Task saveLogin(Accounts account)
        {
            var claims = new List<Claim>()
            {
                new(ClaimTypes.Name, account.Name),
                new (ClaimTypes.Role, account.Role.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal);
        }
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Accounts");
        }

        
       
    }
}
