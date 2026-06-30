using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class ReportsController : AdminControllerBase
{
    public IActionResult Index() => View();
}
