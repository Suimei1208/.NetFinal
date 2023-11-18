using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTechnology_Final.Context;
using NetTechnology_Final.Models;
using NetTechnology_Final.Services.EmailService;
using NetTechnology_Final.Services.Hash;

namespace NetTechnology_Final.Controllers
{
    public class EmailController : Controller
    {
        private readonly ApplicationDbContext _context;
        public readonly IEmailService _emailService;
        public readonly TokenService _tokenService;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public EmailController(IEmailService emailService, ApplicationDbContext context, TokenService tokenService, IHttpContextAccessor httpContextAccessor)
        {
            _emailService = emailService;
            _context = context;
			_tokenService = tokenService;
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
                var NewSale = new Accounts
                {
                    Name = accounts.Name,
                    username = GetUsernameFromEmail(accounts.Email),
                    Email = accounts.Email,
                    Role = Role.Salesperson,
                    Status = Status.InActive,
                    CreateDate = DateTime.UtcNow,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(1)                   
                };
                
                _context.Accounts.Add(NewSale);
                await _context.SaveChangesAsync();

                string token = _tokenService.GenerateToken(NewSale.Email, 1);

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
			if (token != null)
            {
                if (_tokenService.IsTokenExpired(token) == false)
                {
                    // Mã chưa hết hạn, xử lý tiếp theo
                    ViewBag.Message = token;
                    return View();
                }
            }                        
            return RedirectToAction("notfound", "Error");
                                
        }

        [HttpPost]
        public IActionResult CreatePassword(Accounts accounts, string confirmPassword, string email_convert)
        {           
            if (accounts.password != confirmPassword)
            {
                ModelState.AddModelError("password", "The password and confirmation password do not match.");
                return View();
            }
            else
            {
				string ConvertEmail = _tokenService.ValidateToken(email_convert);
				var existingAccount =  _context.Accounts
            .FirstOrDefault(a => a.Email == ConvertEmail);

                if (existingAccount != null)
                {
                    existingAccount.password = PasswordHashingWithSalt.HashPasswordWithKey(accounts.password);
                    existingAccount.Status = Status.Active;

                    _context.SaveChangesAsync();
					return Json(existingAccount.password);
				}
                else return Json(ConvertEmail);
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

        /*public string GenerateRandomToken()
        {
            const int tokenLength = 10;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, tokenLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }*/

        public string GenerateResetPasswordLink(string token)
        {
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            return $"{baseUrl}/Email/CreatePassword?token={token}";
        }

    }
}
