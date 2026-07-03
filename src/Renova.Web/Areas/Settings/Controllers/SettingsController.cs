using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Settings.Controllers;

[Area("Settings")]
public sealed class SettingsController : Controller
{
    public IActionResult Index() => View();
}
