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
using NetTechnology_Final.Services.IMG;

namespace NetTechnology_Final.Controllers
{
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly reCAPTCHA.AspNetCore.IRecaptchaService recaptchaService;
        private readonly IBlobService _blobService;

        public AccountsController(ApplicationDbContext context, reCAPTCHA.AspNetCore.IRecaptchaService recaptchaService, IBlobService blobService)
        {
            _context = context;
            this.recaptchaService = recaptchaService;
            _blobService = blobService;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var acc = _context.Accounts.Find(id);
            if (acc == null)
            {
                return NotFound();
            }
            return View(acc);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int Id)
        {
            var acc = _context.Accounts.Find(Id);
            if (acc == null)
            {
                return NotFound();
            }
            _context.Accounts.Remove(acc);
            await _context.SaveChangesAsync();

            return RedirectToAction("List", "Accounts");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var acc = _context.Accounts.Find(id);
            if (acc == null)
            {
                return NotFound();
            }
            return View(acc);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Accounts accounts)
        {
             var acc = _context.Accounts.Find(id);
             if (acc == null)
             {
                 return NotFound();
             }

             acc.Status = accounts.Status;
             acc.Name = accounts.Name;
             acc.Role = accounts.Role;
            //Nếu có avatar rồi thì xóa
            if(accounts.AvatarFile != null && accounts.AvatarFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(acc.Avatar))
                {
                    // Nếu khác avatar là mặc định thì cần xóa
                    if(_blobService.TryGetBlobNameFromUrl(acc.Avatar) != "origin.png")
                    {
                        await _blobService.DeleteBlobAsync(_blobService.TryGetBlobNameFromUrl(acc.Avatar));
                    }
                }
            
                acc.Avatar = await _blobService.UploadBlobAsync(accounts.AvatarFile);
            }

            await _context.SaveChangesAsync();

             ModelState.AddModelError("Status", "Thay đổi hoàn tất!");

             return View(acc);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Details(int id)
        {
            var acc = _context.Accounts.Find(id);
            if (acc == null)
            {
                return NotFound();
            }
            return View(acc);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult List()
        {
            return View(_context.Accounts);
        }

        public IActionResult UserEdit(int id)
        {
            var acc = _context.Accounts.Find(id);
            if (acc == null)
            {
                return NotFound();
            }
            return View(acc);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UserEdit(int id, Accounts accounts)
        {
            // Đổi 
            var acc = _context.Accounts.Find(id);
            if (acc == null)
            {
                return NotFound();
            }
            acc.Status = accounts.Status;
            acc.Name = accounts.Name;
            if (accounts.AvatarFile != null && accounts.AvatarFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(acc.Avatar))
                {
                    await _blobService.DeleteBlobAsync(_blobService.TryGetBlobNameFromUrl(acc.Avatar));
                }

                acc.Avatar = await _blobService.UploadBlobAsync(accounts.AvatarFile);

            }
            await _context.SaveChangesAsync();

            // Cập nhật lại cái tên trên cái thanh kia
            var NameUser = (ClaimsIdentity)User.Identity;
            // Tìm claim có kiểu ClaimTypes.Name trong danh tính người dùng
            var userNameClaim = NameUser.FindFirst(ClaimTypes.Name);
            if (userNameClaim != null)
            {
                NameUser.RemoveClaim(userNameClaim);  // Xóa claim cũ
                NameUser.AddClaim(new Claim(ClaimTypes.Name, acc.Name));  // Thêm claim mới
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(NameUser));
            }

            return View(acc);
        }


        [Authorize]
        public IActionResult ChangePassword(int id)
        {
            var acc = _context.Accounts.Find(id);
            if (acc == null)
            {
                return NotFound();
            }
            return View(acc);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id,  string old_password, string new_password, string confirmPassword)
        {
            string hashedPassword = PasswordHashingWithSalt.HashPasswordWithKey(old_password);
            var acc = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.password == hashedPassword);
            if(acc != null)
            {
                if(confirmPassword == new_password)
                {
                    acc.password = PasswordHashingWithSalt.HashPasswordWithKey(new_password);
                    await _context.SaveChangesAsync();

                    // đổi thành công rồi thì đăng xuất đăng nhập lại
                    HttpContext.SignOutAsync();
                    return RedirectToAction("Login", "Accounts");
                }
                else
                {
                    ModelState.AddModelError("password", "The password and confirmation password do not match.");
                    return View(acc);
                }
            }
            return View();
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
                    /*var recaptchaResponse = await this.recaptchaService.Validate(Request);
                    if (!recaptchaResponse.success)
                    {
                        ModelState.AddModelError("Role", "Please click on I'm not a robot");
                        return View();
                    }*/

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
                new (ClaimTypes.Role, account.Role.ToString()),
                new(ClaimTypes.NameIdentifier, account.Id.ToString())
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
