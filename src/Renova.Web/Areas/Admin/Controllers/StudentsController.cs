using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class StudentsController : AdminControllerBase
{
    public IActionResult Index() => View();
}
