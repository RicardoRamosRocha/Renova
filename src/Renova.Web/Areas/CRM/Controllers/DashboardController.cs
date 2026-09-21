using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.CRM.ViewModels.Dashboard;
using Renova.Web.Areas.CRM.ViewModels.Students;
using Renova.Web.Services;

namespace Renova.Web.Areas.CRM.Controllers;

[Area("CRM")]
[Authorize]
public sealed class DashboardController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ICurrentTenantService currentTenantService) : Controller
{
    private const string MissingTenantMessage = "Não foi possível identificar a instituição atual. Entre novamente ou contate o administrador.";
    private const int DashboardListSize = 5;
    private const int AppointmentWindowDays = 7;

    public async Task<IActionResult> Index()
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return View(new CrmDashboardViewModel());
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var tenant = tenantId.Value;
        var now = DateTime.UtcNow;
        var appointmentUntil = now.AddDays(AppointmentWindowDays);

        var activeStudentsQuery = db.Students
            .AsNoTracking()
            .Where(item => item.TenantId == tenant && item.Status != StudentStatuses.Inactive);

        var activeAdmissionsQuery = db.Admissions
            .AsNoTracking()
            .Where(item => item.TenantId == tenant &&
                           item.Student.TenantId == tenant &&
                           item.AdmissionStatus == AdmissionStatus.Active);

        var familyQuery = db.FamilyMembers
            .AsNoTracking()
            .Where(item => item.TenantId == tenant && item.Student.TenantId == tenant);

        var appointmentsQuery = db.Appointments
            .AsNoTracking()
            .Where(item =>
                item.Student.TenantId == tenant &&
                (item.Professional == null || item.Professional.TenantId == tenant) &&
                item.Status == (int)AppointmentStatus.Scheduled &&
                item.ScheduledAt >= now &&
                item.ScheduledAt < appointmentUntil);

        var model = new CrmDashboardViewModel
        {
            ActiveStudents = await activeStudentsQuery.CountAsync(),
            ActiveAdmissions = await activeAdmissionsQuery.CountAsync(),
            ActiveFamilies = await familyQuery.CountAsync(),
            UpcomingAppointments = await appointmentsQuery.CountAsync(),
            UpcomingAppointmentItems = await appointmentsQuery
                .OrderBy(item => item.ScheduledAt)
                .ThenBy(item => item.Id)
                .Take(DashboardListSize)
                .Select(item => new CrmDashboardAppointmentViewModel
                {
                    Id = item.Id,
                    StudentId = item.StudentId,
                    StudentName = item.Student.Person != null ? item.Student.Person.FullName : item.Student.FullName,
                    ScheduledAt = item.ScheduledAt,
                    ProfessionalName = item.Professional == null
                        ? null
                        : item.Professional.Person != null ? item.Professional.Person.FullName : item.Professional.FullName,
                    Status = (AppointmentStatus)item.Status
                })
                .ToListAsync(),
            RecentAdmissions = await db.Admissions
                .AsNoTracking()
                .Where(item => item.TenantId == tenant && item.Student.TenantId == tenant)
                .OrderByDescending(item => item.AdmissionDate)
                .ThenByDescending(item => item.Id)
                .Take(DashboardListSize)
                .Select(item => new CrmDashboardAdmissionViewModel
                {
                    Id = item.Id,
                    StudentId = item.StudentId,
                    StudentName = item.Student.Person != null ? item.Student.Person.FullName : item.Student.FullName,
                    AdmissionDate = item.AdmissionDate,
                    Status = item.AdmissionStatus,
                    DischargeDate = item.DischargeDate
                })
                .ToListAsync(),
            RecentStudents = await db.Students
                .AsNoTracking()
                .Where(item => item.TenantId == tenant && item.Status != StudentStatuses.Inactive)
                .OrderByDescending(item => item.CreatedAt)
                .ThenByDescending(item => item.Id)
                .Take(DashboardListSize)
                .Select(item => new CrmDashboardStudentViewModel
                {
                    Id = item.Id,
                    Name = item.Person != null ? item.Person.FullName : item.FullName,
                    Status = item.Status,
                    CreatedAt = item.CreatedAt,
                    ActiveAdmissionId = item.Admissions
                        .Where(admission => admission.AdmissionStatus == AdmissionStatus.Active && admission.TenantId == tenant)
                        .OrderByDescending(admission => admission.AdmissionDate)
                        .Select(admission => (Guid?)admission.Id)
                        .FirstOrDefault()
                })
                .ToListAsync(),
            RecentFamilies = await familyQuery
                .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
                .ThenBy(item => item.Id)
                .Take(DashboardListSize)
                .Select(item => new CrmDashboardFamilyViewModel
                {
                    Id = item.Id,
                    StudentId = item.StudentId,
                    Name = item.Person != null ? item.Person.FullName : item.FullName,
                    StudentName = item.Student.Person != null ? item.Student.Person.FullName : item.Student.FullName,
                    UpdatedAt = item.UpdatedAt,
                    CreatedAt = item.CreatedAt
                })
                .ToListAsync()
        };

        return View(model);
    }
}
