using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.CRM.ViewModels.Dashboard;
using Renova.Web.Areas.CRM.ViewModels.Students;
using Renova.Web.Services;

namespace Renova.Web.Areas.CRM.Controllers;

[Area("CRM")]
public sealed class DashboardController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ICurrentTenantService currentTenantService) : Controller
{
    private const string MissingTenantMessage = "Não foi possível identificar a instituição atual. Entre novamente ou contate o administrador.";

    public async Task<IActionResult> Index()
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return View(new CrmDashboardViewModel());
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var today = DateTime.Today;
        var monthStart = DateTime.SpecifyKind(new DateTime(today.Year, today.Month, 1), DateTimeKind.Utc);
        var nextMonth = monthStart.AddMonths(1);
        var todayStart = DateTime.SpecifyKind(today, DateTimeKind.Utc);
        var tomorrow = todayStart.AddDays(1);

        var studentQuery = db.Students
            .AsNoTracking()
            .Include(item => item.Person)
            .Where(item => item.TenantId == tenantId.Value);

        var admissionQuery = db.Admissions
            .AsNoTracking()
            .Include(item => item.Student)
                .ThenInclude(student => student.Person)
            .Where(item => item.TenantId == tenantId.Value);

        var model = new CrmDashboardViewModel
        {
            ActiveStudents = await studentQuery.CountAsync(item => item.Status != StudentStatuses.Inactive),
            AdmissionsThisMonth = await admissionQuery.CountAsync(item => item.AdmissionDate >= monthStart && item.AdmissionDate < nextMonth),
            ExpectedDischarges = await admissionQuery.CountAsync(item =>
                item.ExpectedDischargeDate.HasValue &&
                item.ExpectedDischargeDate.Value >= todayStart &&
                item.AdmissionStatus == AdmissionStatus.Active),
            CompletedDischarges = await admissionQuery.CountAsync(item =>
                item.DischargeDate.HasValue &&
                item.DischargeDate.Value >= monthStart &&
                item.DischargeDate.Value < nextMonth &&
                item.AdmissionStatus == AdmissionStatus.Discharged),
            Transfers = await admissionQuery.CountAsync(item =>
                item.DischargeDate.HasValue &&
                item.DischargeDate.Value >= monthStart &&
                item.DischargeDate.Value < nextMonth &&
                item.AdmissionStatus == AdmissionStatus.Transferred),
            BirthdaysThisMonth = await studentQuery.CountAsync(item =>
                item.Person != null && item.Person.BirthDate.HasValue
                    ? item.Person.BirthDate.Value.Month == today.Month
                    : item.BirthDate.Month == today.Month),
            LatestStudents = await studentQuery
                .OrderByDescending(item => item.CreatedAt)
                .Take(5)
                .Select(item => new CrmDashboardStudentViewModel
                {
                    Id = item.Id,
                    Name = item.Person != null ? item.Person.FullName : item.FullName,
                    PhotoUrl = item.Person != null && item.Person.PhotoUrl != null ? item.Person.PhotoUrl : item.PhotoPath,
                    CreatedAt = item.CreatedAt
                })
                .ToListAsync(),
            UpcomingDischarges = await admissionQuery
                .Where(item => item.ExpectedDischargeDate.HasValue && item.ExpectedDischargeDate.Value >= todayStart)
                .OrderBy(item => item.ExpectedDischargeDate)
                .Take(5)
                .Select(item => new CrmDashboardAdmissionViewModel
                {
                    StudentId = item.StudentId,
                    StudentName = item.Student.Person != null ? item.Student.Person.FullName : item.Student.FullName,
                    ExpectedDischargeDate = item.ExpectedDischargeDate!.Value,
                    ResponsibleProfessional = item.ResponsibleProfessional
                })
                .ToListAsync(),
            TodayAppointments = await db.Appointments
                .AsNoTracking()
                .Include(item => item.Student)
                    .ThenInclude(student => student.Person)
                .Include(item => item.Professional)
                .Where(item =>
                    item.Student.TenantId == tenantId.Value &&
                    item.ScheduledAt >= todayStart &&
                    item.ScheduledAt < tomorrow)
                .OrderBy(item => item.ScheduledAt)
                .Take(6)
                .Select(item => new CrmDashboardAppointmentViewModel
                {
                    StudentId = item.StudentId,
                    StudentName = item.Student.Person != null ? item.Student.Person.FullName : item.Student.FullName,
                    ScheduledAt = item.ScheduledAt,
                    ProfessionalName = item.Professional != null ? item.Professional.FullName : null
                })
                .ToListAsync()
        };

        return View(model);
    }
}
