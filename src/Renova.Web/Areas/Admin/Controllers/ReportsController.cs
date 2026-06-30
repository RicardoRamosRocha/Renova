using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public sealed class ReportsController : Controller
{
    public IActionResult Index() => View();
}
