using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.CRM.ViewModels.Families;
using Renova.Web.Services;

namespace Renova.Web.Areas.CRM.Controllers;

[Area("CRM")]
public sealed class FamiliesController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ICurrentTenantService currentTenantService,
    IPhotoService photoService) : Controller
{
    private const string MissingTenantMessage = "Não foi possível identificar a instituição atual. Entre novamente ou contate o administrador.";

    public async Task<IActionResult> Index(Guid? studentId)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return View(Array.Empty<FamilyMember>());
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        IQueryable<FamilyMember> query = db.FamilyMembers
            .AsNoTracking()
            .Include(item => item.Person)
            .Include(item => item.Student)
            .Where(item => item.TenantId == tenantId.Value);

        if (studentId.HasValue)
        {
            query = query.Where(item => item.StudentId == studentId.Value);
        }

        ViewBag.StudentId = studentId;

        var members = await query
            .OrderBy(item => item.Student.FullName)
            .ThenByDescending(item => item.IsResponsible)
            .ThenBy(item => item.FullName)
            .ToListAsync();

        return View(members);
    }

    public async Task<IActionResult> Create(Guid studentId)
    {
        var model = await CreateFormAsync(studentId);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FamilyMemberFormViewModel model)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            ModelState.AddModelError(string.Empty, MissingTenantMessage);
            return View(model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students.FirstOrDefaultAsync(item => item.Id == model.StudentId && item.TenantId == tenantId.Value);
        if (student is null)
        {
            return NotFound();
        }

        model.StudentName = student.DisplayName;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? photoPath;
        try
        {
            photoPath = model.RemovePhoto ? null : await photoService.SavePhotoAsync(model.Photo, "families");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Photo), ex.Message);
            return View(model);
        }

        var timestamp = DateTime.UtcNow;
        var member = new FamilyMember
        {
            TenantId = tenantId.Value,
            StudentId = student.Id,
            FullName = model.FullName.Trim(),
            Relationship = model.Relationship.Trim(),
            RelationshipType = model.RelationshipType,
            Phone = model.Phone.Trim(),
            Email = Trim(model.Email),
            PhotoPath = photoPath,
            IsResponsible = model.IsResponsible,
            CanAccessPortal = model.CanAccessPortal,
            CreatedAt = timestamp
        };

        member.SyncPersonFromLegacyFields(timestamp);
        db.FamilyMembers.Add(member);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            photoService.DeletePhoto(photoPath);
            ModelState.AddModelError(string.Empty, "Não foi possível salvar o familiar. Verifique dados duplicados ou obrigatórios.");
            return View(model);
        }

        TempData["Success"] = "Familiar vinculado com sucesso.";
        return RedirectToAction("Details", "Students", new { id = student.Id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction("Index", "Students");
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var member = await db.FamilyMembers
            .Include(item => item.Person)
            .Include(item => item.Student)
            .FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);

        return member is null ? NotFound() : View(ToForm(member));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, FamilyMemberFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            ModelState.AddModelError(string.Empty, MissingTenantMessage);
            return View(model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var member = await db.FamilyMembers
            .Include(item => item.Person)
            .Include(item => item.Student)
            .FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);

        if (member is null)
        {
            return NotFound();
        }

        model.StudentName = member.Student.DisplayName;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var oldPhotoPath = member.DisplayPhotoUrl;
        try
        {
            if (model.RemovePhoto)
            {
                photoService.DeletePhoto(oldPhotoPath);
                member.PhotoPath = null;
            }
            else
            {
                member.PhotoPath = await photoService.SavePhotoAsync(model.Photo, "families", oldPhotoPath);
            }
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Photo), ex.Message);
            model.PhotoPath = oldPhotoPath;
            return View(model);
        }

        var timestamp = DateTime.UtcNow;
        member.FullName = model.FullName.Trim();
        member.Relationship = model.Relationship.Trim();
        member.RelationshipType = model.RelationshipType;
        member.Phone = model.Phone.Trim();
        member.Email = Trim(model.Email);
        member.IsResponsible = model.IsResponsible;
        member.CanAccessPortal = model.CanAccessPortal;
        member.UpdatedAt = timestamp;
        member.SyncPersonFromLegacyFields(timestamp, markPersonAsUpdated: true);

        await db.SaveChangesAsync();

        TempData["Success"] = "Familiar atualizado com sucesso.";
        return RedirectToAction("Details", "Students", new { id = member.StudentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(Guid id)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction("Index", "Students");
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var member = await db.FamilyMembers
            .Include(item => item.Person)
            .FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);

        if (member is null)
        {
            return NotFound();
        }

        member.IsDeleted = true;
        member.UpdatedAt = DateTime.UtcNow;
        if (member.Person is not null)
        {
            member.Person.IsActive = false;
            member.Person.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        TempData["Success"] = "Familiar arquivado com segurança.";
        return RedirectToAction("Details", "Students", new { id = member.StudentId });
    }

    private async Task<FamilyMemberFormViewModel?> CreateFormAsync(Guid studentId)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return null;
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == studentId && item.TenantId == tenantId.Value);

        return student is null
            ? null
            : new FamilyMemberFormViewModel
            {
                StudentId = student.Id,
                StudentName = student.DisplayName
            };
    }

    private static FamilyMemberFormViewModel ToForm(FamilyMember member) => new()
    {
        Id = member.Id,
        StudentId = member.StudentId,
        StudentName = member.Student.DisplayName,
        FullName = member.DisplayName,
        Relationship = member.Relationship,
        RelationshipType = member.RelationshipType,
        Phone = member.DisplayPhone,
        Email = member.DisplayEmail,
        PhotoPath = member.DisplayPhotoUrl,
        IsResponsible = member.IsResponsible,
        CanAccessPortal = member.CanAccessPortal
    };

    private static string? Trim(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
