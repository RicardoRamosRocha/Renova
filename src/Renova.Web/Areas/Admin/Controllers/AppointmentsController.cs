using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class AppointmentsController : AdminControllerBase
{
    public IActionResult Index() => View();
}
