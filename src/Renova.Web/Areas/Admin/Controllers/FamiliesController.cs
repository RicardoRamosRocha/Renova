using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class FamiliesController : AdminControllerBase
{
    public IActionResult Index() => View();
}
