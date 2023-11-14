using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTechnology_Final.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using MimeKit;

namespace NetTechnology_Final.Controllers
{
    public class AccountsController : Controller
    {
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

            if (account.username == "admin" && account.password == "admin")
            {
                await saveLogin(account);
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        private async Task saveLogin(Accounts account)
        {
            string role = "Salesperson";
            if (account.username == "admin") role = "Admin";
            var claims = new List<Claim>()
            {
                new (ClaimTypes.Role, role.ToString())
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
