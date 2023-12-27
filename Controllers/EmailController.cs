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
                    password = PasswordHashingWithSalt.HashPasswordWithKey(GetUsernameFromEmail(accounts.Email)),
                    Email = accounts.Email,
                    Role = Role.Salesperson,
                    Status = Status.InActive,
                    Avatar = "https://avatarfinal.blob.core.windows.net/alluser/origin.png",
                    CreateDate = DateTime.UtcNow                                   
                };            
                _context.Accounts.Add(NewSale);
                await _context.SaveChangesAsync();

                string token = _tokenService.GenerateToken(NewSale.Email, 60);

				_emailService.SendEmail(request, accounts, GenerateResetPasswordLink(token));
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View();
            }          
        }

        [Authorize(Roles = nameof(Role.Admin))]
        public IActionResult Create()
        {
            return View();
        }


        //CreatePassword page
        [AllowAnonymous]
        public IActionResult CreatePassword()
        {
			string token = _httpContextAccessor.HttpContext.Request.Query["token"];
			if (token != null)
            {
               bool isExpired = _tokenService.IsTokenExpired(token);
                if (isExpired == false)
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

                    _context.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                else return View();
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
       
        public string GenerateResetPasswordLink(string token)
        {
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            return $"{baseUrl}/Email/CreatePassword?token={token}";
        }

    }
}
