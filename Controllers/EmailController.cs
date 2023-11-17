using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTechnology_Final.Context;
using NetTechnology_Final.Models;
using NetTechnology_Final.Services.EmailService;


namespace NetTechnology_Final.Controllers
{
    public class EmailController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public readonly IEmailService _emailService;
        public EmailController(IEmailService emailService, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _emailService = emailService;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(EmailDto request, Accounts accounts, DateTime dateTime)
        {
            var Sale = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email == accounts.Email);
            if (Sale == null)
            {
                var token = GenerateRandomToken();
                var NewSale = new Accounts
                {
                    Name = accounts.Name,
                    username = GetUsernameFromEmail(accounts.Email),
                    Email = accounts.Email,
                    Role = Role.Salesperson,
                    Status = Status.InActive,
                    CreateDate = DateTime.UtcNow,
                    Token = token,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(1)                   
                };
                
                _context.Accounts.Add(NewSale);
                await _context.SaveChangesAsync();

                _emailService.SendEmail(request, accounts, GenerateResetPasswordLink(token));
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View();
            }          
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }


        //CreatePassword page
        public IActionResult CreatePassword()
        {
            string token = _httpContextAccessor.HttpContext.Request.Query["token"];

            var tokenInfo = _context.Accounts.FirstOrDefault(a => a.Token == token);

            if (tokenInfo != null)
            {
                DateTime tokenExpiration = DateTime.UtcNow;

                if (tokenInfo.TokenExpiration != null)
                {
                    if (tokenInfo.TokenExpiration > tokenExpiration)
                    {
                    // Mã chưa hết hạn, xử lý tiếp theo
                        ViewBag.Message = tokenInfo.Email;
                        return View();
                    }
                    else
                    {
                        // Mã đã hết hạn, chuyển hướng đến trang notfound
                        return RedirectToAction("notfound", "Error");
                    }
                }              
            }                        
            return RedirectToAction("notfound", "Error");
                                
        }

        [HttpPost]
        public IActionResult CreatePassword(Accounts accounts, string confirmPassword, string email)
        {           
            if (accounts.password != confirmPassword)
            {
                ModelState.AddModelError("password", "The password and confirmation password do not match.");
                return View();
            }
            else
            {
                var existingAccount =  _context.Accounts
            .FirstOrDefault(a => a.Email == email);

                if (existingAccount != null)
                {
                    existingAccount.password = accounts.password;
                    existingAccount.Status = Status.Active;

                    _context.SaveChangesAsync();
                    return RedirectToAction("Login", "Accounts");
                }
                else return Json(existingAccount);
            }               
        }

        public string GetUsernameFromEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            int atIndex = email.IndexOf('@');
            if (atIndex >= 0)
            {
                return email.Substring(0, atIndex);
            }

            return null; 
        }

        public string GenerateRandomToken()
        {
            const int tokenLength = 10;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, tokenLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string GenerateResetPasswordLink(string token)
        {
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            return $"{baseUrl}/Email/CreatePassword?token={token}";
        }

    }
}
