using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class UsersController : AdminControllerBase
{
    public IActionResult Index() => View();
}
