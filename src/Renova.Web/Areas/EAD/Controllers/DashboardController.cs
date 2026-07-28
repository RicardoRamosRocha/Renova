using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.EAD.ViewModels.Dashboard;
using Renova.Web.Services;

namespace Renova.Web.Areas.EAD.Controllers;

[Area("EAD")]
[Authorize]
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

        var courses = await db.Courses
            .AsNoTracking()
            .Include(course => course.Modules)
                .ThenInclude(module => module.Lessons)
                    .ThenInclude(lesson => lesson.ProgressEntries)
                        .ThenInclude(item => item.Student)
            .Include(course => course.Certificates)
                .ThenInclude(item => item.Student)
            .AsSplitQuery()
            .ToListAsync();

        var lessonCount = courses.SelectMany(course => course.Modules).SelectMany(module => module.Lessons).Count();
        var completedLessons = progress.Count(item => item.CompletedAt.HasValue || item.WatchedPercentage >= 100);
        var learningMinutes = progress.Sum(item =>
            (item.Lesson?.DurationInMinutes ?? 0) *
            Math.Clamp(item.WatchedPercentage, 0, 100) / 100m);
        var studentsNeedingAttention = progress
            .GroupBy(item => item.StudentId)
            .Count(group =>
                group.Average(item => item.WatchedPercentage) < 40 ||
                group.Max(item => item.UpdatedAt ?? item.CreatedAt) < DateTime.UtcNow.AddDays(-14));

        var topCourses = courses
            .Select(course =>
            {
                var courseProgress = course.Modules
                    .SelectMany(module => module.Lessons)
                    .SelectMany(lesson => lesson.ProgressEntries)
                    .Where(item => item.Student.TenantId == tenantId.Value)
                    .ToList();
                var completed = courseProgress.Count(item => item.CompletedAt.HasValue || item.WatchedPercentage >= 100);
                return new CoursePerformanceViewModel
                {
                    Id = course.Id,
                    Title = course.Title,
                    Trail = CoursesController.InferTrail(course.Title),
                    Students = courseProgress.Select(item => item.StudentId).Distinct().Count(),
                    AverageProgress = courseProgress.Count == 0 ? 0 : ClampPercent((int)Math.Round(courseProgress.Average(item => item.WatchedPercentage))),
                    CompletionRate = courseProgress.Count == 0 ? 0 : ClampPercent((int)Math.Round(completed * 100m / courseProgress.Count))
                };
            })
            .Where(item => item.Students > 0)
            .OrderByDescending(item => item.AverageProgress)
            .ThenByDescending(item => item.CompletionRate)
            .Take(5)
            .ToList();

        return View(new EadDashboardViewModel
        {
            ActiveCourses = courses.Count(item => item.IsActive),
            TotalCourses = courses.Count,
            AvailableLessons = lessonCount,
            StudyingStudents = progress.Select(item => item.StudentId).Distinct().Count(),
            CompletedLessons = completedLessons,
            LearningHours = (int)Math.Round(learningMinutes / 60m),
            Certificates = await db.Certificates.CountAsync(item => item.Student.TenantId == tenantId.Value),
            AverageEngagement = progress.Count == 0 ? 0 : ClampPercent((int)Math.Round(progress.Average(item => item.WatchedPercentage))),
            CompletionRate = progress.Count == 0 ? 0 : ClampPercent((int)Math.Round(completedLessons * 100m / progress.Count)),
            StudentsNeedingAttention = studentsNeedingAttention,
            TopCourses = topCourses,
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

    private static int ClampPercent(int value) => Math.Clamp(value, 0, 100);
}
