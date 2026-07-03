using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.CRM.Controllers;

[Area("CRM")]
public sealed class AppointmentsController : Controller
{
    public IActionResult Index() => View();
}
