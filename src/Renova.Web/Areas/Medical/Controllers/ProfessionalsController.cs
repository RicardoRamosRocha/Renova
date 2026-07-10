using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.Medical.ViewModels.Professionals;
using Renova.Web.Services;
using Renova.Web.ViewModels;

namespace Renova.Web.Areas.Medical.Controllers;

[Area("Medical")]
public sealed class ProfessionalsController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ICurrentTenantService currentTenantService) : Controller
{
    private const string MissingTenantMessage = "Não foi possível identificar a instituição atual. Entre novamente ou contate o administrador.";

    public async Task<IActionResult> Index(string? search, bool? active, int page = 1)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return View(new PagedResult<Professional>
            {
                Items = [],
                Page = 1,
                PageSize = 10,
                TotalItems = 0
            });
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        IQueryable<Professional> query = db.Professionals
            .AsNoTracking()
            .Include(item => item.Person)
            .Where(item => item.TenantId == tenantId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(item =>
                item.FullName.ToLower().Contains(term) ||
                (item.Person != null && item.Person.FullName.ToLower().Contains(term)) ||
                (item.Person != null && item.Person.Email != null && item.Person.Email.ToLower().Contains(term)) ||
                item.Specialty.ToLower().Contains(term) ||
                item.RegistrationNumber.ToLower().Contains(term));
        }

        if (active.HasValue)
        {
            query = query.Where(item => item.IsActive == active.Value);
        }

        ViewBag.Search = search;
        ViewBag.Active = active;
        ViewBag.Total = await db.Professionals.CountAsync(item => item.TenantId == tenantId.Value);
        ViewBag.ActiveCount = await db.Professionals.CountAsync(item => item.TenantId == tenantId.Value && item.IsActive);
        ViewBag.InactiveCount = await db.Professionals.CountAsync(item => item.TenantId == tenantId.Value && !item.IsActive);
        ViewBag.Specialties = await db.Professionals
            .Where(item => item.TenantId == tenantId.Value)
            .Select(item => item.Specialty)
            .Distinct()
            .CountAsync();

        const int pageSize = 10;
        page = Math.Max(1, page);
        var totalItems = await query.CountAsync();
        var professionals = await query
            .OrderBy(item => item.Person != null ? item.Person.FullName : item.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(new PagedResult<Professional>
        {
            Items = professionals,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction(nameof(Index));
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var professional = await db.Professionals
            .AsNoTracking()
            .Include(item => item.Person)
            .FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);
        return professional is null ? NotFound() : View(professional);
    }

    public IActionResult Create() => View(new ProfessionalFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProfessionalFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            ModelState.AddModelError(string.Empty, MissingTenantMessage);
            return View(model);
        }

        var timestamp = DateTime.UtcNow;
        var professional = new Professional
        {
            FullName = model.FullName.Trim(),
            Specialty = model.Specialty.Trim(),
            RegistrationNumber = model.RegistrationNumber.Trim(),
            Phone = model.Phone.Trim(),
            Email = model.Email?.Trim(),
            IsActive = model.IsActive,
            TenantId = tenantId.Value,
            CreatedAt = timestamp
        };

        professional.SyncPersonFromLegacyFields(timestamp);

        db.Professionals.Add(professional);

        if (!await TrySaveAsync(db, "Profissional cadastrado com sucesso."))
        {
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction(nameof(Index));
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var professional = await db.Professionals
            .Include(item => item.Person)
            .FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);
        return professional is null ? NotFound() : View(ToForm(professional));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProfessionalFormViewModel model)
    {
        if (id != model.Id || !ModelState.IsValid)
        {
            return View(model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            ModelState.AddModelError(string.Empty, MissingTenantMessage);
            return View(model);
        }

        var professional = await db.Professionals
            .Include(item => item.Person)
            .FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);
        if (professional is null)
        {
            return NotFound();
        }

        var timestamp = DateTime.UtcNow;
        professional.FullName = model.FullName.Trim();
        professional.Specialty = model.Specialty.Trim();
        professional.RegistrationNumber = model.RegistrationNumber.Trim();
        professional.Phone = model.Phone.Trim();
        professional.Email = model.Email?.Trim();
        professional.IsActive = model.IsActive;
        professional.UpdatedAt = timestamp;
        professional.SyncPersonFromLegacyFields(timestamp, markPersonAsUpdated: true);

        if (!await TrySaveAsync(db, "Profissional atualizado com sucesso."))
        {
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inactivate(Guid id)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction(nameof(Index));
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var professional = await db.Professionals.FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);
        if (professional is null)
        {
            return NotFound();
        }

        professional.IsActive = false;
        professional.UpdatedAt = DateTime.UtcNow;
        await TrySaveAsync(db, "Profissional inativado com sucesso.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction(nameof(Index));
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var professional = await db.Professionals.FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);
        if (professional is null)
        {
            return NotFound();
        }

        try
        {
            db.Professionals.Remove(professional);
            await db.SaveChangesAsync();
            TempData["Success"] = "Profissional excluído com sucesso.";
        }
        catch (DbUpdateException)
        {
            professional.IsActive = false;
            professional.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            TempData["Success"] = "Profissional possui vínculos e foi inativado com segurança.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> TrySaveAsync(AppDbContext db, string successMessage)
    {
        try
        {
            await db.SaveChangesAsync();
            TempData["Success"] = successMessage;
            return true;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Não foi possível salvar. Verifique se o registro profissional já existe.");
            return false;
        }
    }

    private static ProfessionalFormViewModel ToForm(Professional professional) => new()
    {
        Id = professional.Id,
        FullName = professional.DisplayName,
        Specialty = professional.Specialty,
        RegistrationNumber = professional.RegistrationNumber,
        Phone = professional.DisplayPhone,
        Email = professional.DisplayEmail,
        IsActive = professional.IsActive
    };
}
