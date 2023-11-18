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
using NetTechnology_Final.Services.Hash;
using Newtonsoft.Json;
using NetTechnology_Final.Services;
using reCAPTCHA.AspNetCore;

namespace NetTechnology_Final.Controllers
{
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly reCAPTCHA.AspNetCore.IRecaptchaService recaptchaService;

        public AccountsController(ApplicationDbContext context, reCAPTCHA.AspNetCore.IRecaptchaService recaptchaService)
        {
            _context = context;
            this.recaptchaService = recaptchaService;
        }

        [AllowAnonymous]
        [OnlyUnauthenticated]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [OnlyUnauthenticated]
        public async Task<IActionResult> Login(Accounts account, [FromServices] reCAPTCHA.AspNetCore.IRecaptchaService recaptchaService)
        {
            string hashedPassword = PasswordHashingWithSalt.HashPasswordWithKey(account.password);

            var adminAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.username == account.username && a.password == hashedPassword);

            if (adminAccount != null)
            {
                if (adminAccount.Status == Status.Active)
                {
                    var recaptchaResponse = await this.recaptchaService.Validate(Request);
                    if (!recaptchaResponse.success)
                    {
                        ModelState.AddModelError("Role", "Please click on I'm not a robot");
                        return View();
                    }

                    await saveLogin(adminAccount);
                    return RedirectToAction("Index", "Home");
                }
                else if (adminAccount.Status == Status.InActive)
                {
                    ModelState.AddModelError("Role", "Your account is inactive! Please contact admin to activate the account and create a password.");
                }
                else if (adminAccount.Status == Status.Block)
                {
                    ModelState.AddModelError("Role", "Your account is blocked! Please contact admin to unblock.");
                }
            }
            else
            {
                ModelState.AddModelError("Role", "Incorrect username or password. Please try again.");
            }

            return View();
        }

        private async Task saveLogin(Accounts account)
        {
            var claims = new List<Claim>()
            {
                new(ClaimTypes.Name, account.Name),
                new(ClaimTypes.Email, account.Email),
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
