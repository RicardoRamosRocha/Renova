using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public sealed class MedicalRecordsController : Controller
{
    public IActionResult Index() => View();
}
