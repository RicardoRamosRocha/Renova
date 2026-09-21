using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Renova.Web.Areas.CRM.Controllers;

[Area("CRM")]
[Authorize]
public sealed class ProfessionalsController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Professionals", new { area = "Medical" });

    public IActionResult Details(Guid id) => RedirectToAction("Details", "Professionals", new { area = "Medical", id });

    public IActionResult Create() => RedirectToAction("Create", "Professionals", new { area = "Medical" });

    public IActionResult Edit(Guid id) => RedirectToAction("Edit", "Professionals", new { area = "Medical", id });
}
