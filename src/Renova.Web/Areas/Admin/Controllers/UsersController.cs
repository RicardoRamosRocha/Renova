using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public sealed class UsersController : Controller
{
    public IActionResult Index() => View();
}
