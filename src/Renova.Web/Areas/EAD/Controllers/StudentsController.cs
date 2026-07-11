using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.EAD.ViewModels.Students;
using Renova.Web.Services;

namespace Renova.Web.Areas.EAD.Controllers;

[Area("EAD")]
public sealed class StudentsController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ICurrentTenantService currentTenantService) : Controller
{
    private const string MissingTenantMessage = "Não foi possível identificar a instituição atual. Entre novamente ou contate o administrador.";

    public async Task<IActionResult> Dashboard(Guid? studentId)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return View(new StudentLearningDashboardViewModel());
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var students = await db.Students
            .AsNoTracking()
            .Include(item => item.Person)
            .Where(item => item.TenantId == tenantId.Value)
            .OrderBy(item => item.Person != null ? item.Person.FullName : item.FullName)
            .Select(item => new StudentOptionViewModel
            {
                Id = item.Id,
                Name = item.Person != null ? item.Person.FullName : item.FullName
            })
            .ToListAsync();

        var selectedStudentId = studentId ?? students.FirstOrDefault()?.Id;
        if (!selectedStudentId.HasValue)
        {
            return View(new StudentLearningDashboardViewModel { Students = students });
        }

        var student = await db.Students
            .AsNoTracking()
            .Include(item => item.Person)
            .Include(item => item.ProgressEntries)
                .ThenInclude(progress => progress.Lesson)
                    .ThenInclude(lesson => lesson.CourseModule)
                        .ThenInclude(module => module.Course)
            .Include(item => item.Certificates)
                .ThenInclude(certificate => certificate.Course)
            .FirstOrDefaultAsync(item => item.Id == selectedStudentId.Value && item.TenantId == tenantId.Value);

        if (student is null)
        {
            return NotFound();
        }

        var courses = student.ProgressEntries
            .GroupBy(item => item.Lesson.CourseModule.Course)
            .Select(group =>
            {
                var totalLessons = group.Key.Modules.SelectMany(module => module.Lessons).Count();
                var completed = group.Count(item => item.CompletedAt.HasValue || item.WatchedPercentage >= 100);
                return new StudentCourseProgressViewModel
                {
                    CourseId = group.Key.Id,
                    Title = group.Key.Title,
                    Trail = CoursesController.InferTrail(group.Key.Title),
                    Progress = group.Any() ? (int)Math.Round(group.Average(item => item.WatchedPercentage)) : 0,
                    CompletedLessons = completed,
                    TotalLessons = totalLessons
                };
            })
            .OrderByDescending(item => item.Progress)
            .ToList();

        var trails = courses
            .GroupBy(item => item.Trail)
            .Select(group => new StudentTrailProgressViewModel
            {
                Trail = group.Key,
                Courses = group.Count(),
                Progress = (int)Math.Round(group.Average(item => item.Progress))
            })
            .ToList();

        var completedLessons = student.ProgressEntries.Count(item => item.CompletedAt.HasValue || item.WatchedPercentage >= 100);
        var overall = courses.Count == 0 ? 0 : (int)Math.Round(courses.Average(item => item.Progress));

        return View(new StudentLearningDashboardViewModel
        {
            StudentId = student.Id,
            StudentName = student.DisplayName,
            PhotoUrl = student.DisplayPhotoUrl,
            Students = students,
            OverallProgress = overall,
            Certificates = student.Certificates.Count,
            CompletedLessons = completedLessons,
            NextActivity = courses.OrderBy(item => item.Progress).FirstOrDefault(item => item.Progress < 100)?.Title,
            Courses = courses,
            Trails = trails,
            Goals = BuildGoals(courses),
            Achievements = BuildAchievements(courses, trails, student.Certificates.Count, completedLessons)
        });
    }

    private static IReadOnlyList<StudentGoalViewModel> BuildGoals(IReadOnlyList<StudentCourseProgressViewModel> courses)
    {
        return courses
            .OrderBy(item => item.Progress)
            .Take(4)
            .Select(item => new StudentGoalViewModel
            {
                Objective = item.Trail,
                Activity = item.Title,
                Responsible = CoursesController.InferTeacher(item.Title),
                Status = item.Progress >= 100 ? "Concluída" : item.Progress > 0 ? "Em andamento" : "Pendente",
                Progress = item.Progress
            })
            .ToList();
    }

    private static IReadOnlyList<StudentAchievementViewModel> BuildAchievements(
        IReadOnlyList<StudentCourseProgressViewModel> courses,
        IReadOnlyList<StudentTrailProgressViewModel> trails,
        int certificates,
        int completedLessons)
    {
        return
        [
            new() { Title = "Primeiro Curso", Description = "Iniciou ao menos um curso.", Achieved = courses.Count > 0, Icon = "ph-play-circle" },
            new() { Title = "Primeira Trilha", Description = "Iniciou uma trilha terapêutica.", Achieved = trails.Count > 0, Icon = "ph-path" },
            new() { Title = "Plano concluído", Description = "Concluiu todas as metas EAD atuais.", Achieved = courses.Count > 0 && courses.All(item => item.Progress >= 100), Icon = "ph-target" },
            new() { Title = "Certificado conquistado", Description = "Recebeu ao menos um certificado.", Achieved = certificates > 0, Icon = "ph-certificate" },
            new() { Title = "7 atividades", Description = "Concluiu sete aulas ou atividades.", Achieved = completedLessons >= 7, Icon = "ph-medal" }
        ];
    }
}
