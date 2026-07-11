using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.EAD.ViewModels.Dashboard;
using Renova.Web.Services;

namespace Renova.Web.Areas.EAD.Controllers;

[Area("EAD")]
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
            return View(new EadDashboardViewModel());
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var progress = await db.StudentProgress
            .AsNoTracking()
            .Include(item => item.Student)
                .ThenInclude(student => student.Person)
            .Include(item => item.Lesson)
                .ThenInclude(lesson => lesson.CourseModule)
                    .ThenInclude(module => module.Course)
            .Where(item => item.Student.TenantId == tenantId.Value)
            .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
            .ToListAsync();

        var trails = progress
            .GroupBy(item => CoursesController.InferTrail(item.Lesson.CourseModule.Course.Title))
            .Select(group => new TrailSummaryViewModel
            {
                Name = group.Key,
                Courses = group.Select(item => item.Lesson.CourseModule.CourseId).Distinct().Count(),
                Students = group.Select(item => item.StudentId).Distinct().Count(),
                AverageProgress = (int)Math.Round(group.Average(item => item.WatchedPercentage)),
                Icon = CoursesController.InferIcon(group.Key)
            })
            .OrderBy(item => item.Name)
            .ToList();

        return View(new EadDashboardViewModel
        {
            ActiveCourses = await db.Courses.CountAsync(item => item.IsActive),
            StudyingStudents = progress.Select(item => item.StudentId).Distinct().Count(),
            CompletedLessons = progress.Count(item => item.CompletedAt.HasValue),
            Certificates = await db.Certificates.CountAsync(item => item.Student.TenantId == tenantId.Value),
            AverageEngagement = progress.Count == 0 ? 0 : (int)Math.Round(progress.Average(item => item.WatchedPercentage)),
            Trails = trails,
            RecentActivities = progress
                .Take(8)
                .Select(item => new RecentLearningActivityViewModel
                {
                    StudentName = item.Student.DisplayName,
                    CourseTitle = item.Lesson.CourseModule.Course.Title,
                    LessonTitle = item.Lesson.Title,
                    Progress = item.WatchedPercentage,
                    OccurredAt = item.UpdatedAt ?? item.CreatedAt
                })
                .ToList()
        });
    }
}
