using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Finance.Controllers;

[Area("Finance")]
public sealed class FinanceController : Controller
{
    public IActionResult Index() => View();
}
