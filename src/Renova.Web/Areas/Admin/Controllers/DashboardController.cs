using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class DashboardController : AdminControllerBase
{
    public IActionResult Index()
    {
        return View();
    }
}
