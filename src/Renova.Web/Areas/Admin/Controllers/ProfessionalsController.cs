using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class ProfessionalsController : AdminControllerBase
{
    public IActionResult Index() => View();
}
