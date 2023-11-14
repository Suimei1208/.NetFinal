using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace NetTechnology_Final.Controllers
{
    public class OnlyUnauthenticatedAttribute : TypeFilterAttribute
    {
        public OnlyUnauthenticatedAttribute() : base(typeof(Myfilter))
        {

        }

        private class Myfilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (context.HttpContext.User.Identity.IsAuthenticated)
                {
                    context.Result = new RedirectToActionResult("Index", "Home", null);

                }
            }
        }
    }
}
