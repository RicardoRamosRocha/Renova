using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.Medical.ViewModels.Professionals;
using Renova.Web.ViewModels;

namespace Renova.Web.Areas.Medical.Controllers;

[Area("Medical")]
public sealed class ProfessionalsController(IDbContextFactory<AppDbContext> dbContextFactory) : Controller
{
    public async Task<IActionResult> Index(string? search, bool? active, int page = 1)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var query = db.Professionals.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(item =>
                item.FullName.ToLower().Contains(term) ||
                item.Specialty.ToLower().Contains(term) ||
                item.RegistrationNumber.ToLower().Contains(term));
        }

        if (active.HasValue)
        {
            query = query.Where(item => item.IsActive == active.Value);
        }

        ViewBag.Search = search;
        ViewBag.Active = active;
        ViewBag.Total = await db.Professionals.CountAsync();
        ViewBag.ActiveCount = await db.Professionals.CountAsync(item => item.IsActive);
        ViewBag.InactiveCount = await db.Professionals.CountAsync(item => !item.IsActive);
        ViewBag.Specialties = await db.Professionals.Select(item => item.Specialty).Distinct().CountAsync();

        const int pageSize = 10;
        page = Math.Max(1, page);
        var totalItems = await query.CountAsync();
        var professionals = await query
            .OrderBy(item => item.FullName)
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
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var professional = await db.Professionals.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
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
        db.Professionals.Add(new Professional
        {
            FullName = model.FullName.Trim(),
            Specialty = model.Specialty.Trim(),
            RegistrationNumber = model.RegistrationNumber.Trim(),
            Phone = model.Phone.Trim(),
            Email = model.Email?.Trim(),
            IsActive = model.IsActive
        });

        if (!await TrySaveAsync(db, "Profissional cadastrado com sucesso."))
        {
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var professional = await db.Professionals.FindAsync(id);
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
        var professional = await db.Professionals.FindAsync(id);
        if (professional is null)
        {
            return NotFound();
        }

        professional.FullName = model.FullName.Trim();
        professional.Specialty = model.Specialty.Trim();
        professional.RegistrationNumber = model.RegistrationNumber.Trim();
        professional.Phone = model.Phone.Trim();
        professional.Email = model.Email?.Trim();
        professional.IsActive = model.IsActive;
        professional.UpdatedAt = DateTime.UtcNow;

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
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var professional = await db.Professionals.FindAsync(id);
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
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var professional = await db.Professionals.FindAsync(id);
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
        FullName = professional.FullName,
        Specialty = professional.Specialty,
        RegistrationNumber = professional.RegistrationNumber,
        Phone = professional.Phone,
        Email = professional.Email,
        IsActive = professional.IsActive
    };
}
