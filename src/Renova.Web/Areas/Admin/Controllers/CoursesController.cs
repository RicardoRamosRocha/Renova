using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class CoursesController : AdminControllerBase
{
    public IActionResult Index() => View();
}
