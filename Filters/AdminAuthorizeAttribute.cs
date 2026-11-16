using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DBWineStore.Filters
{
    // Gắn [AdminAuthorize] lên Controller hoặc Action để chỉ cho Role = Admin vào.
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Chưa có UserID trong Session = chưa đăng nhập
            if (httpContext.Session == null || httpContext.Session["UserID"] == null)
            {
                return false;
            }

            string role = Convert.ToString(httpContext.Session["RoleName"])
                       ?? Convert.ToString(httpContext.Session["Role"]);
            return role == "Admin";
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Chưa đăng nhập -> về trang Login
            if (filterContext.HttpContext.Session == null
                || filterContext.HttpContext.Session["UserID"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Account", action = "Login" }));
                return;
            }

            // Đã đăng nhập nhưng không phải Admin -> 403
            filterContext.Result = new HttpStatusCodeResult(
                403,
                "Bạn không có quyền truy cập trang quản trị.");
        }
    }
}
