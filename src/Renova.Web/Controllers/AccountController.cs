using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Controllers;

[AllowAnonymous]
public sealed class AccountController : Controller
{
    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl = null, string? error = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(GetSafeReturnUrl(returnUrl));
        }

        ViewBag.ReturnUrl = GetSafeReturnUrl(returnUrl);
        ViewBag.HasError = !string.IsNullOrWhiteSpace(error);

        return View();
    }

    private static string GetSafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return "/Admin";
        }

        return returnUrl[0] == '/'
            && (returnUrl.Length == 1 || returnUrl[1] is not '/' and not '\\')
            && !returnUrl.Contains("://", StringComparison.Ordinal)
                ? returnUrl
                : "/Admin";
    }
}
