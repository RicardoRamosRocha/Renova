using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.Medical.Controllers;

[Area("Medical")]
public sealed class MedicalRecordsController : Controller
{
    public IActionResult Index() => View();
}
