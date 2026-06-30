using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class FinanceController : AdminControllerBase
{
    public IActionResult Index() => View();
}
