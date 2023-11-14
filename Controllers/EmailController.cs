using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NetTechnology_Final.Models;
using NetTechnology_Final.Services.EmailService;

namespace NetTechnology_Final.Controllers
{
    public class EmailController : Controller
    {
        public readonly IEmailService _emailService;
        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create(EmailDto request, Accounts accounts)
        {
            _emailService.SendEmail(request, accounts);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }
    }
}
