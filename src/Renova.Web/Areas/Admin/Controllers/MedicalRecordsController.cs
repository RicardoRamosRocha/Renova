using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Admin.Controllers;

public sealed class MedicalRecordsController : AdminControllerBase
{
    public IActionResult Index() => View();
}
