using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class RolesController : AdminControllerBase
{
    public IActionResult Index() => View();
}
