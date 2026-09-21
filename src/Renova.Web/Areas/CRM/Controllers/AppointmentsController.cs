using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.CRM.ViewModels.Appointments;
using Renova.Web.Services;

namespace Renova.Web.Areas.CRM.Controllers;

[Area("CRM")]
[Authorize]
public sealed class AppointmentsController(IDbContextFactory<AppDbContext> dbContextFactory, ICurrentTenantService currentTenantService) : Controller
{
    private const string MissingTenantMessage = "Não foi possível identificar a instituição atual. Entre novamente ou contate o administrador.";
    private const int PageSize = 10;

    public async Task<IActionResult> Index(string? search, DateTime? from, DateTime? to, int? status, Guid? professionalId, int page = 1)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue) { TempData["Error"] = MissingTenantMessage; return View(new AppointmentIndexViewModel { Page = 1, PageSize = PageSize }); }
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var query = ApplyFilters(TenantAppointments(db, tenantId.Value).AsNoTracking(), search, from, to, status, professionalId);
        var totalItems = await query.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize)));
        var appointments = await query.OrderBy(item => item.ScheduledAt).ThenBy(item => item.Id).Skip((page - 1) * PageSize).Take(PageSize)
            .Select(item => new AppointmentIndexItemViewModel
            {
                Id = item.Id, StudentId = item.StudentId,
                StudentName = item.Student.Person != null ? item.Student.Person.FullName : item.Student.FullName,
                StudentPhotoUrl = item.Student.Person != null ? item.Student.Person.PhotoUrl : item.Student.PhotoPath,
                ProfessionalName = item.Professional == null ? null : item.Professional.Person != null ? item.Professional.Person.FullName : item.Professional.FullName,
                ScheduledAt = item.ScheduledAt, Status = item.Status, StatusLabel = StatusLabel(item.Status), Notes = item.Notes
            }).ToListAsync();
        var metrics = TenantAppointments(db, tenantId.Value);
        var professionals = await db.Professionals.AsNoTracking().Where(item => item.TenantId == tenantId.Value && item.IsActive)
            .OrderBy(item => item.Person != null ? item.Person.FullName : item.FullName)
            .Select(item => new AppointmentOptionViewModel { Id = item.Id, Name = item.Person != null ? item.Person.FullName : item.FullName }).ToListAsync();
        return View(new AppointmentIndexViewModel
        {
            Search = search, From = from, To = to, Status = status, ProfessionalId = professionalId, Page = page, PageSize = PageSize, TotalItems = totalItems,
            Total = await metrics.CountAsync(), Scheduled = await metrics.CountAsync(item => item.Status == (int)AppointmentStatus.Scheduled),
            Completed = await metrics.CountAsync(item => item.Status == (int)AppointmentStatus.Completed), Cancelled = await metrics.CountAsync(item => item.Status == (int)AppointmentStatus.Cancelled),
            Professionals = professionals, Appointments = appointments
        });
    }

    public async Task<IActionResult> Create(Guid? studentId = null)
    {
        var model = await BuildFormAsync(await currentTenantService.GetTenantIdAsync(), studentId, null);
        return model is null ? RedirectWithTenantError() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentFormViewModel model) => await SaveAsync(model, null);

    public async Task<IActionResult> Details(Guid id)
    {
        var appointment = await FindTenantAppointmentAsync(id, true);
        return appointment is null ? NotFound() : View(ToDetails(appointment));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        var appointment = tenantId.HasValue ? await FindTenantAppointmentAsync(id, false) : null;
        if (appointment is null) return NotFound();
        return View(await BuildFormAsync(tenantId, appointment.StudentId, appointment));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AppointmentFormViewModel model) { model.Id = id; return await SaveAsync(model, id); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var appointment = await FindTenantAppointmentAsync(id, false);
        if (appointment is null) return NotFound();
        appointment.Status = (int)AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;
        await using var db = await dbContextFactory.CreateDbContextAsync();
        db.Attach(appointment);
        await db.SaveChangesAsync();
        TempData["Success"] = "Atendimento cancelado com sucesso.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<IActionResult> SaveAsync(AppointmentFormViewModel model, Guid? id)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue) return RedirectWithTenantError();
        if (model.ScheduledAt == default) ModelState.AddModelError(nameof(model.ScheduledAt), "Informe a data e hora.");
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students.FirstOrDefaultAsync(item => item.Id == model.StudentId && item.TenantId == tenantId.Value);
        if (student is null) ModelState.AddModelError(nameof(model.StudentId), "Selecione um acolhido válido da instituição atual.");
        if (model.ProfessionalId.HasValue && !await db.Professionals.AnyAsync(item => item.Id == model.ProfessionalId && item.TenantId == tenantId.Value && item.IsActive)) ModelState.AddModelError(nameof(model.ProfessionalId), "Selecione um profissional ativo da instituição atual.");
        if (!Enum.IsDefined(model.Status)) ModelState.AddModelError(nameof(model.Status), "Status inválido.");
        if (!ModelState.IsValid)
        {
            model.StudentName = student?.DisplayName ?? model.StudentName;
            await PopulateOptionsAsync(db, tenantId.Value, model);
            return View(id.HasValue ? "Edit" : "Create", model);
        }
        Appointment appointment;
        if (id.HasValue) { appointment = await TenantAppointments(db, tenantId.Value).FirstOrDefaultAsync(item => item.Id == id.Value) ?? null!; if (appointment is null) return NotFound(); }
        else { appointment = new Appointment { CreatedAt = DateTime.UtcNow }; db.Appointments.Add(appointment); }
        appointment.StudentId = model.StudentId; appointment.ProfessionalId = model.ProfessionalId; appointment.ScheduledAt = ToUtc(model.ScheduledAt);
        appointment.Status = (int)model.Status; appointment.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(); appointment.UpdatedAt = id.HasValue ? DateTime.UtcNow : null;
        await db.SaveChangesAsync();
        TempData["Success"] = id.HasValue ? "Atendimento atualizado com sucesso." : "Atendimento criado com sucesso.";
        return RedirectToAction(nameof(Details), new { id = appointment.Id });
    }

    private async Task<AppointmentFormViewModel?> BuildFormAsync(Guid? tenantId, Guid? studentId, Appointment? appointment)
    {
        if (!tenantId.HasValue) return null;
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var model = appointment is null ? new AppointmentFormViewModel { StudentId = studentId ?? Guid.Empty, ScheduledAt = DateTime.Today.AddHours(9) } : new AppointmentFormViewModel
        {
            Id = appointment.Id, StudentId = appointment.StudentId, StudentName = appointment.Student.DisplayName, ProfessionalId = appointment.ProfessionalId,
            ScheduledAt = appointment.ScheduledAt, Status = Enum.IsDefined(typeof(AppointmentStatus), appointment.Status) ? (AppointmentStatus)appointment.Status : AppointmentStatus.Scheduled, Notes = appointment.Notes
        };
        await PopulateOptionsAsync(db, tenantId.Value, model);
        return model;
    }

    private static async Task PopulateOptionsAsync(AppDbContext db, Guid tenantId, AppointmentFormViewModel model)
    {
        model.Students = await db.Students.AsNoTracking().Where(item => item.TenantId == tenantId && item.Status != 5).OrderBy(item => item.Person != null ? item.Person.FullName : item.FullName)
            .Select(item => new AppointmentOptionViewModel { Id = item.Id, Name = item.Person != null ? item.Person.FullName : item.FullName }).ToListAsync();
        model.Professionals = await db.Professionals.AsNoTracking().Where(item => item.TenantId == tenantId && item.IsActive).OrderBy(item => item.Person != null ? item.Person.FullName : item.FullName)
            .Select(item => new AppointmentOptionViewModel { Id = item.Id, Name = item.Person != null ? item.Person.FullName : item.FullName }).ToListAsync();
    }

    private async Task<Appointment?> FindTenantAppointmentAsync(Guid id, bool asNoTracking)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync(); if (!tenantId.HasValue) return null;
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var query = TenantAppointments(db, tenantId.Value);
        if (asNoTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(item => item.Id == id);
    }

    private static IQueryable<Appointment> TenantAppointments(AppDbContext db, Guid tenantId) => db.Appointments
        .Include(item => item.Student).ThenInclude(item => item.Person).Include(item => item.Professional).ThenInclude(item => item!.Person)
        .Where(item => item.Student.TenantId == tenantId && (item.Professional == null || item.Professional.TenantId == tenantId));

    private static IQueryable<Appointment> ApplyFilters(IQueryable<Appointment> query, string? search, DateTime? from, DateTime? to, int? status, Guid? professionalId)
    {
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim().ToLower(); query = query.Where(item => item.Student.FullName.ToLower().Contains(term) || (item.Student.Person != null && item.Student.Person.FullName.ToLower().Contains(term))); }
        if (from.HasValue) query = query.Where(item => item.ScheduledAt >= DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc));
        if (to.HasValue) query = query.Where(item => item.ScheduledAt < DateTime.SpecifyKind(to.Value.Date.AddDays(1), DateTimeKind.Utc));
        if (status.HasValue) query = query.Where(item => item.Status == status.Value);
        if (professionalId.HasValue) query = query.Where(item => item.ProfessionalId == professionalId.Value);
        return query;
    }

    private IActionResult RedirectWithTenantError() { TempData["Error"] = MissingTenantMessage; return RedirectToAction(nameof(Index)); }
    private static AppointmentDetailsViewModel ToDetails(Appointment item) => new() { Id = item.Id, StudentId = item.StudentId, StudentName = item.Student.DisplayName, ProfessionalId = item.ProfessionalId, ProfessionalName = item.Professional?.DisplayName, ScheduledAt = item.ScheduledAt, Status = Enum.IsDefined(typeof(AppointmentStatus), item.Status) ? (AppointmentStatus)item.Status : 0, Notes = item.Notes, CreatedAt = item.CreatedAt, UpdatedAt = item.UpdatedAt };
    public static string StatusLabel(int status) => status switch { 1 => "Agendado", 2 => "Concluído", 3 => "Cancelado", _ => "Indefinido" };
    private static DateTime ToUtc(DateTime value) => value.Kind switch { DateTimeKind.Utc => value, DateTimeKind.Local => value.ToUniversalTime(), _ => DateTime.SpecifyKind(value, DateTimeKind.Utc) };
}
