using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.CRM.ViewModels.Appointments;
using Renova.Web.Services;

namespace Renova.Web.Areas.CRM.Controllers;

[Area("CRM")]
public sealed class AppointmentsController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ICurrentTenantService currentTenantService) : Controller
{
    private const string MissingTenantMessage = "Não foi possível identificar a instituição atual. Entre novamente ou contate o administrador.";

    public async Task<IActionResult> Index(DateTime? date)
    {
        var selectedDate = date?.Date ?? DateTime.Today;
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return View(new AppointmentIndexViewModel { Date = selectedDate });
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var from = DateTime.SpecifyKind(selectedDate, DateTimeKind.Utc);
        var to = from.AddDays(1);

        var query = db.Appointments
            .AsNoTracking()
            .Include(item => item.Student)
                .ThenInclude(student => student.Person)
            .Include(item => item.Professional)
            .Where(item =>
                item.Student.TenantId == tenantId.Value &&
                item.ScheduledAt >= from &&
                item.ScheduledAt < to);

        var appointments = await query
            .OrderBy(item => item.ScheduledAt)
            .Select(item => new AppointmentIndexItemViewModel
            {
                StudentId = item.StudentId,
                StudentName = item.Student.Person != null ? item.Student.Person.FullName : item.Student.FullName,
                StudentPhotoUrl = item.Student.Person != null && item.Student.Person.PhotoUrl != null ? item.Student.Person.PhotoUrl : item.Student.PhotoPath,
                ProfessionalName = item.Professional != null ? item.Professional.FullName : null,
                ScheduledAt = item.ScheduledAt,
                Status = item.Status,
                Notes = item.Notes
            })
            .ToListAsync();

        return View(new AppointmentIndexViewModel
        {
            Date = selectedDate,
            Total = appointments.Count,
            Scheduled = appointments.Count(item => item.Status == 1),
            Completed = appointments.Count(item => item.Status == 2),
            Cancelled = appointments.Count(item => item.Status == 3),
            Appointments = appointments
        });
    }
}
