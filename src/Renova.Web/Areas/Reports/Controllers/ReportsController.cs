using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Reports.Controllers;

[Area("Reports")]
public sealed class ReportsController : Controller
{
    public IActionResult Index() => View();
}
