using Microsoft.AspNetCore.Mvc;
using Renova.Web.Services;

namespace Renova.Web.Areas.CRM.Controllers;

[Area("CRM")]
public sealed class FamiliesController(ICurrentTenantService currentTenantService) : Controller
{
    private const string MissingTenantMessage = "Não foi possível identificar a instituição atual. Entre novamente ou contate o administrador.";

    public async Task<IActionResult> Index()
    {
        if (!await HasTenantAsync())
        {
            TempData["Error"] = MissingTenantMessage;
        }

        return View();
    }

    private async Task<bool> HasTenantAsync()
    {
        return (await currentTenantService.GetTenantIdAsync()).HasValue;
    }
}
