using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class SettingsController : AdminControllerBase
{
    public IActionResult Index() => View();
}
