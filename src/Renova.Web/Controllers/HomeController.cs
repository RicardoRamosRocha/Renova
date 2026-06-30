using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
