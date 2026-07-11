using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.EAD.ViewModels.Courses;
using Renova.Web.Services;
using Renova.Web.ViewModels;

namespace Renova.Web.Areas.EAD.Controllers;

[Area("EAD")]
public sealed class CoursesController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ICurrentTenantService currentTenantService) : Controller
{
    private const string MissingTenantMessage = "Não foi possível identificar a instituição atual. Entre novamente ou contate o administrador.";

    public async Task<IActionResult> Index(string? search, bool? active, string? trail, int page = 1)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return View(new CourseIndexViewModel());
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var coursesQuery = db.Courses
            .AsNoTracking()
            .Include(course => course.Modules)
                .ThenInclude(module => module.Lessons)
                    .ThenInclude(lesson => lesson.ProgressEntries)
                        .ThenInclude(progress => progress.Student)
            .Include(course => course.Certificates)
                .ThenInclude(certificate => certificate.Student)
            .AsSplitQuery();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            coursesQuery = coursesQuery.Where(course =>
                course.Title.ToLower().Contains(term) ||
                course.Description.ToLower().Contains(term));
        }

        if (active.HasValue)
        {
            coursesQuery = coursesQuery.Where(course => course.IsActive == active.Value);
        }

        var allCourses = await coursesQuery
            .OrderBy(course => course.Title)
            .ToListAsync();

        var items = allCourses
            .Select(course => ToIndexItem(course, tenantId.Value))
            .Where(item => string.IsNullOrWhiteSpace(trail) || item.Trail == trail)
            .ToList();

        const int pageSize = 10;
        page = Math.Max(1, page);

        var pagedItems = items
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var tenantProgress = await db.StudentProgress
            .AsNoTracking()
            .Where(progress => progress.Student.TenantId == tenantId.Value)
            .ToListAsync();

        return View(new CourseIndexViewModel
        {
            Search = search,
            Active = active,
            Trail = trail,
            TotalCourses = await db.Courses.CountAsync(),
            ActiveCourses = await db.Courses.CountAsync(course => course.IsActive),
            StudyingStudents = tenantProgress.Select(progress => progress.StudentId).Distinct().Count(),
            Certificates = await db.Certificates.CountAsync(certificate => certificate.Student.TenantId == tenantId.Value),
            AverageProgress = tenantProgress.Count == 0 ? 0 : (int)Math.Round(tenantProgress.Average(progress => progress.WatchedPercentage)),
            Trails = allCourses.Select(course => InferTrail(course.Title)).Distinct().OrderBy(item => item).ToList(),
            Courses = new PagedResult<CourseIndexItemViewModel>
            {
                Items = pagedItems,
                Page = page,
                PageSize = pageSize,
                TotalItems = items.Count
            }
        });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction(nameof(Index));
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var course = await db.Courses
            .AsNoTracking()
            .Include(item => item.Modules)
                .ThenInclude(module => module.Lessons)
                    .ThenInclude(lesson => lesson.ProgressEntries)
                        .ThenInclude(progress => progress.Student)
            .Include(item => item.Certificates)
                .ThenInclude(certificate => certificate.Student)
            .AsSplitQuery()
            .FirstOrDefaultAsync(item => item.Id == id);

        return course is null ? NotFound() : View(ToDetails(course, tenantId.Value));
    }

    public IActionResult Create() => View(new CourseFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        db.Courses.Add(new Course
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            IsActive = model.IsActive
        });

        await db.SaveChangesAsync();
        TempData["Success"] = "Curso cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var course = await db.Courses.FindAsync(id);
        return course is null ? NotFound() : View(ToForm(course));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CourseFormViewModel model)
    {
        if (id != model.Id || !ModelState.IsValid)
        {
            return View(model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var course = await db.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        course.Title = model.Title.Trim();
        course.Description = model.Description.Trim();
        course.IsActive = model.IsActive;
        course.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        TempData["Success"] = "Curso atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inactivate(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var course = await db.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        course.IsActive = false;
        course.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Success"] = "Curso arquivado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var course = await db.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        try
        {
            db.Courses.Remove(course);
            await db.SaveChangesAsync();
            TempData["Success"] = "Curso excluído com sucesso.";
        }
        catch (DbUpdateException)
        {
            course.IsActive = false;
            course.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            TempData["Success"] = "Curso possui vínculos e foi arquivado com segurança.";
        }

        return RedirectToAction(nameof(Index));
    }

    private static CourseIndexItemViewModel ToIndexItem(Course course, Guid tenantId)
    {
        var lessons = course.Modules.SelectMany(module => module.Lessons).ToList();
        var progress = lessons
            .SelectMany(lesson => lesson.ProgressEntries)
            .Where(item => item.Student.TenantId == tenantId)
            .ToList();

        var duration = lessons.Sum(lesson => lesson.DurationInMinutes);

        return new CourseIndexItemViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            IsActive = course.IsActive,
            Trail = InferTrail(course.Title),
            Category = InferCategory(course.Title),
            Teacher = InferTeacher(course.Title),
            Level = InferLevel(course.Modules.Count),
            WorkloadHours = Math.Max(1, (int)Math.Ceiling(duration / 60m)),
            DurationInMinutes = duration,
            Modules = course.Modules.Count,
            Lessons = lessons.Count,
            Students = progress.Select(item => item.StudentId).Distinct().Count(),
            Certificates = course.Certificates.Count(item => item.Student.TenantId == tenantId),
            AverageProgress = progress.Count == 0 ? 0 : (int)Math.Round(progress.Average(item => item.WatchedPercentage)),
            Icon = InferIcon(course.Title)
        };
    }

    private static CourseDetailsViewModel ToDetails(Course course, Guid tenantId)
    {
        var item = ToIndexItem(course, tenantId);
        return new CourseDetailsViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            IsActive = course.IsActive,
            Trail = item.Trail,
            Category = item.Category,
            Teacher = item.Teacher,
            WorkloadHours = item.WorkloadHours,
            Lessons = item.Lessons,
            Students = item.Students,
            Certificates = item.Certificates,
            AverageProgress = item.AverageProgress,
            Modules = course.Modules
                .OrderBy(module => module.Order)
                .Select(module => new CourseModuleDetailsViewModel
                {
                    Title = module.Title,
                    Description = module.Description,
                    Order = module.Order,
                    Lessons = module.Lessons
                        .OrderBy(lesson => lesson.Order)
                        .Select(lesson =>
                        {
                            var progress = lesson.ProgressEntries
                                .Where(entry => entry.Student.TenantId == tenantId)
                                .ToList();
                            return new CourseLessonDetailsViewModel
                            {
                                Title = lesson.Title,
                                VideoProvider = string.IsNullOrWhiteSpace(lesson.VideoProvider) ? "Estrutura pronta" : lesson.VideoProvider,
                                DurationInMinutes = lesson.DurationInMinutes,
                                Students = progress.Select(entry => entry.StudentId).Distinct().Count(),
                                AverageProgress = progress.Count == 0 ? 0 : (int)Math.Round(progress.Average(entry => entry.WatchedPercentage))
                            };
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    private static CourseFormViewModel ToForm(Course course) => new()
    {
        Id = course.Id,
        Title = course.Title,
        Description = course.Description,
        IsActive = course.IsActive,
        Status = course.IsActive ? "Publicado" : "Arquivado"
    };

    public static string InferTrail(string text)
    {
        var value = text.ToLowerInvariant();
        if (value.Contains("reca")) return "Prevenção à Recaída";
        if (value.Contains("fam")) return "Família";
        if (value.Contains("espiritual")) return "Espiritualidade";
        if (value.Contains("trabalho") || value.Contains("mercado")) return "Mercado de Trabalho";
        if (value.Contains("vida") || value.Contains("projeto")) return "Projeto de Vida";
        if (value.Contains("emoc")) return "Controle Emocional";
        if (value.Contains("social") || value.Contains("reinser")) return "Reintegração Social";
        if (value.Contains("auto")) return "Autoconhecimento";
        return "Primeiros Dias";
    }

    public static string InferCategory(string text) => InferTrail(text);

    public static string InferTeacher(string text)
    {
        var trail = InferTrail(text);
        return trail switch
        {
            "Prevenção à Recaída" or "Controle Emocional" or "Autoconhecimento" => "Equipe Terapêutica",
            "Família" or "Reintegração Social" => "Serviço Social",
            "Espiritualidade" => "Coordenação Terapêutica",
            "Mercado de Trabalho" or "Projeto de Vida" => "Equipe Pedagógica",
            _ => "Equipe Renova"
        };
    }

    public static string InferIcon(string text) => InferTrail(text) switch
    {
        "Prevenção à Recaída" => "ph-shield-check",
        "Família" => "ph-users-four",
        "Espiritualidade" => "ph-sparkle",
        "Mercado de Trabalho" => "ph-briefcase",
        "Projeto de Vida" => "ph-target",
        "Controle Emocional" => "ph-heart",
        "Reintegração Social" => "ph-handshake",
        "Autoconhecimento" => "ph-brain",
        _ => "ph-seedling"
    };

    private static string InferLevel(int modules) => modules switch
    {
        <= 1 => "Inicial",
        <= 3 => "Intermediário",
        _ => "Avançado"
    };
}
