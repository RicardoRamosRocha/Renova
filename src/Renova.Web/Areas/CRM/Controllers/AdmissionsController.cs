using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.CRM.ViewModels.Admissions;
using Renova.Web.Areas.CRM.ViewModels.Students;
using Renova.Web.Services;

namespace Renova.Web.Areas.CRM.Controllers;

[Area("CRM")]
public sealed class AdmissionsController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ICurrentTenantService currentTenantService) : Controller
{
    private const string MissingTenantMessage = "Não foi possível identificar a instituição atual. Entre novamente ou contate o administrador.";

    public async Task<IActionResult> Index(Guid studentId)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction("Index", "Students");
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == studentId && item.TenantId == tenantId.Value);

        if (student is null)
        {
            return NotFound();
        }

        ViewBag.StudentId = student.Id;
        ViewBag.StudentName = student.DisplayName;

        var admissions = await db.Admissions
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId.Value && item.StudentId == studentId)
            .OrderByDescending(item => item.AdmissionDate)
            .ToListAsync();

        return View(admissions);
    }

    public async Task<IActionResult> Create(Guid studentId)
    {
        var model = await CreateFormAsync(studentId);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdmissionFormViewModel model)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            ModelState.AddModelError(string.Empty, MissingTenantMessage);
            return View(model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students.FirstOrDefaultAsync(item =>
            item.Id == model.StudentId &&
            item.TenantId == tenantId.Value &&
            item.Status != StudentStatuses.Inactive);

        if (student is null)
        {
            return NotFound();
        }

        model.StudentName = student.DisplayName;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.AdmissionStatus == AdmissionStatus.Active && await HasActiveAdmissionAsync(db, tenantId.Value, student.Id))
        {
            ModelState.AddModelError(string.Empty, "Este acolhido já possui uma admissão ativa.");
            return View(model);
        }

        var admission = new Admission
        {
            TenantId = tenantId.Value,
            StudentId = student.Id,
            AdmissionDate = DateTime.SpecifyKind(model.AdmissionDate.Date, DateTimeKind.Utc),
            ExpectedDischargeDate = ToUtcDate(model.ExpectedDischargeDate),
            DischargeDate = ToUtcDate(model.DischargeDate),
            AdmissionReason = Trim(model.AdmissionReason),
            DischargeReason = Trim(model.DischargeReason),
            AdmissionStatus = model.AdmissionStatus,
            ReferredBy = Trim(model.ReferredBy),
            ResponsibleProfessional = Trim(model.ResponsibleProfessional),
            DischargeApprovedBy = Trim(model.DischargeApprovedBy),
            Origin = Trim(model.Origin),
            DestinationAfterDischarge = Trim(model.DestinationAfterDischarge),
            Notes = Trim(model.Notes),
            CreatedAt = DateTime.UtcNow
        };

        ApplyStudentStatus(student, admission.AdmissionStatus);

        db.Admissions.Add(admission);
        await db.SaveChangesAsync();

        TempData["Success"] = "Admissão registrada com sucesso.";
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
        var admission = await db.Admissions
            .Include(item => item.Student)
            .FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);

        return admission is null ? NotFound() : View(ToForm(admission));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AdmissionFormViewModel model)
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
        var admission = await db.Admissions
            .Include(item => item.Student)
            .FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);

        if (admission is null)
        {
            return NotFound();
        }

        model.StudentName = admission.Student.DisplayName;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.AdmissionStatus == AdmissionStatus.Active &&
            await HasActiveAdmissionAsync(db, tenantId.Value, admission.StudentId, admission.Id))
        {
            ModelState.AddModelError(string.Empty, "Este acolhido já possui outra admissão ativa.");
            return View(model);
        }

        admission.AdmissionDate = DateTime.SpecifyKind(model.AdmissionDate.Date, DateTimeKind.Utc);
        admission.ExpectedDischargeDate = ToUtcDate(model.ExpectedDischargeDate);
        admission.DischargeDate = ToUtcDate(model.DischargeDate);
        admission.AdmissionReason = Trim(model.AdmissionReason);
        admission.DischargeReason = Trim(model.DischargeReason);
        admission.AdmissionStatus = model.AdmissionStatus;
        admission.ReferredBy = Trim(model.ReferredBy);
        admission.ResponsibleProfessional = Trim(model.ResponsibleProfessional);
        admission.DischargeApprovedBy = Trim(model.DischargeApprovedBy);
        admission.Origin = Trim(model.Origin);
        admission.DestinationAfterDischarge = Trim(model.DestinationAfterDischarge);
        admission.Notes = Trim(model.Notes);
        admission.UpdatedAt = DateTime.UtcNow;

        ApplyStudentStatus(admission.Student, admission.AdmissionStatus);
        await db.SaveChangesAsync();

        TempData["Success"] = "Admissão atualizada com sucesso.";
        return RedirectToAction("Details", "Students", new { id = admission.StudentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Discharge(Guid id)
    {
        return await ChangeStatusAsync(id, AdmissionStatus.Discharged, "Alta registrada com sucesso.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(Guid id)
    {
        return await ChangeStatusAsync(id, AdmissionStatus.Transferred, "Transferência registrada com sucesso.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        return await ChangeStatusAsync(id, AdmissionStatus.Cancelled, "Admissão cancelada com segurança.");
    }

    private async Task<AdmissionFormViewModel?> CreateFormAsync(Guid studentId)
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
            : new AdmissionFormViewModel
            {
                StudentId = student.Id,
                StudentName = student.DisplayName,
                AdmissionDate = DateTime.Today,
                AdmissionStatus = AdmissionStatus.Active
            };
    }

    private async Task<IActionResult> ChangeStatusAsync(Guid id, AdmissionStatus status, string message)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction("Index", "Students");
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var admission = await db.Admissions
            .Include(item => item.Student)
            .FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);

        if (admission is null)
        {
            return NotFound();
        }

        admission.AdmissionStatus = status;
        admission.UpdatedAt = DateTime.UtcNow;

        if (status is AdmissionStatus.Discharged or AdmissionStatus.Transferred)
        {
            admission.DischargeDate ??= DateTime.UtcNow.Date;
        }

        ApplyStudentStatus(admission.Student, status);
        await db.SaveChangesAsync();

        TempData["Success"] = message;
        return RedirectToAction("Details", "Students", new { id = admission.StudentId });
    }

    private static AdmissionFormViewModel ToForm(Admission admission) => new()
    {
        Id = admission.Id,
        StudentId = admission.StudentId,
        StudentName = admission.Student.DisplayName,
        AdmissionDate = admission.AdmissionDate,
        ExpectedDischargeDate = admission.ExpectedDischargeDate,
        DischargeDate = admission.DischargeDate,
        AdmissionReason = admission.AdmissionReason,
        DischargeReason = admission.DischargeReason,
        AdmissionStatus = admission.AdmissionStatus,
        ReferredBy = admission.ReferredBy,
        ResponsibleProfessional = admission.ResponsibleProfessional,
        DischargeApprovedBy = admission.DischargeApprovedBy,
        Origin = admission.Origin,
        DestinationAfterDischarge = admission.DestinationAfterDischarge,
        Notes = admission.Notes
    };

    private static async Task<bool> HasActiveAdmissionAsync(
        AppDbContext db,
        Guid tenantId,
        Guid studentId,
        Guid? ignoreAdmissionId = null)
    {
        return await db.Admissions.AnyAsync(item =>
            item.TenantId == tenantId &&
            item.StudentId == studentId &&
            item.Id != ignoreAdmissionId &&
            item.AdmissionStatus == AdmissionStatus.Active);
    }

    private static void ApplyStudentStatus(Student student, AdmissionStatus admissionStatus)
    {
        student.Status = admissionStatus switch
        {
            AdmissionStatus.Active => StudentStatuses.InTreatment,
            AdmissionStatus.Planned => StudentStatuses.DischargePlanned,
            AdmissionStatus.Discharged => StudentStatuses.Discharged,
            AdmissionStatus.Cancelled => StudentStatuses.Inactive,
            AdmissionStatus.Transferred => StudentStatuses.Discharged,
            _ => student.Status
        };
        student.UpdatedAt = DateTime.UtcNow;
    }

    private static DateTime? ToUtcDate(DateTime? value)
    {
        return value.HasValue ? DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc) : null;
    }

    private static string? Trim(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
