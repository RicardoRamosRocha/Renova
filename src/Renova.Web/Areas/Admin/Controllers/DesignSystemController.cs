using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class DesignSystemController : AdminControllerBase
{
    public IActionResult Index() => View();
}
