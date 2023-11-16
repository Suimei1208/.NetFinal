using Microsoft.AspNetCore.Mvc;

namespace NetTechnology_Final.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult notfound()
        {
            return View();
        }
    }
}
