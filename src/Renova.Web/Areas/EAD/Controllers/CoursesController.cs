using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.EAD.ViewModels.Courses;
using Renova.Web.Services;
using Renova.Web.ViewModels;

namespace Renova.Web.Areas.EAD.Controllers;

[Area("EAD")]
[Authorize]
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
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return View(new CourseIndexViewModel
        {
            Search = search,
            Active = active,
            Trail = trail,
            TotalCourses = await db.Courses.CountAsync(),
            ActiveCourses = await db.Courses.CountAsync(course => course.IsActive),
            DraftCourses = await db.Courses.CountAsync(course => !course.IsActive),
            StudyingStudents = tenantProgress.Select(progress => progress.StudentId).Distinct().Count(),
            CompletionsThisMonth = tenantProgress.Count(progress => progress.CompletedAt >= monthStart),
            Certificates = await db.Certificates.CountAsync(certificate => certificate.Student.TenantId == tenantId.Value),
            AverageProgress = tenantProgress.Count == 0 ? 0 : ClampPercent((int)Math.Round(tenantProgress.Average(progress => progress.WatchedPercentage))),
            CompletionRate = tenantProgress.Count == 0 ? 0 : ClampPercent((int)Math.Round(tenantProgress.Count(progress => progress.CompletedAt.HasValue || progress.WatchedPercentage >= 100) * 100m / tenantProgress.Count)),
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
    public Task<IActionResult> Publish(Guid id) => SetActiveAsync(id, true, "Curso publicado com sucesso.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Unpublish(Guid id) => SetActiveAsync(id, false, "Curso despublicado com sucesso.");

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

    public async Task<IActionResult> Content(Guid id)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction(nameof(Index));
        }

        var course = await LoadCourseDetailsAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        return View(new CourseContentViewModel
        {
            CourseId = course.Id,
            CourseTitle = course.Title,
            IsActive = course.IsActive,
            Modules = ToDetails(course, tenantId.Value).Modules
        });
    }

    public async Task<IActionResult> CreateModule(Guid courseId)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var nextOrder = (await db.CourseModules
            .Where(module => module.CourseId == courseId)
            .Select(module => (int?)module.Order)
            .MaxAsync() ?? 0) + 1;
        return View("ModuleForm", new CourseModuleFormViewModel { CourseId = courseId, Order = nextOrder });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateModule(CourseModuleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("ModuleForm", model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        if (!await db.Courses.AnyAsync(course => course.Id == model.CourseId))
        {
            return NotFound();
        }

        if (await db.CourseModules.AnyAsync(module => module.CourseId == model.CourseId && module.Order == model.Order))
        {
            ModelState.AddModelError(nameof(model.Order), "Ja existe um modulo com esta ordem.");
            return View("ModuleForm", model);
        }

        db.CourseModules.Add(new CourseModule
        {
            CourseId = model.CourseId,
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            Order = model.Order
        });

        await db.SaveChangesAsync();
        TempData["Success"] = "Modulo criado com sucesso.";
        return RedirectToAction(nameof(Content), new { id = model.CourseId });
    }

    public async Task<IActionResult> EditModule(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var module = await db.CourseModules.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return module is null
            ? NotFound()
            : View("ModuleForm", new CourseModuleFormViewModel
            {
                Id = module.Id,
                CourseId = module.CourseId,
                Title = module.Title,
                Description = module.Description,
                Order = module.Order
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditModule(Guid id, CourseModuleFormViewModel model)
    {
        if (id != model.Id || !ModelState.IsValid)
        {
            return View("ModuleForm", model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var module = await db.CourseModules.FirstOrDefaultAsync(item => item.Id == id && item.CourseId == model.CourseId);
        if (module is null)
        {
            return NotFound();
        }

        module.Title = model.Title.Trim();
        module.Description = model.Description.Trim();
        if (module.Order != model.Order &&
            await db.CourseModules.AnyAsync(item => item.CourseId == model.CourseId && item.Order == model.Order && item.Id != id))
        {
            ModelState.AddModelError(nameof(model.Order), "Ja existe um modulo com esta ordem.");
            return View("ModuleForm", model);
        }

        module.Order = model.Order;
        module.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Success"] = "Modulo atualizado com sucesso.";
        return RedirectToAction(nameof(Content), new { id = model.CourseId });
    }

    public async Task<IActionResult> CreateLesson(Guid courseId, Guid moduleId)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var nextOrder = (await db.Lessons
            .Where(lesson => lesson.CourseModuleId == moduleId)
            .Select(lesson => (int?)lesson.Order)
            .MaxAsync() ?? 0) + 1;
        return View("LessonForm", new LessonFormViewModel { CourseId = courseId, ModuleId = moduleId, Order = nextOrder });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLesson(LessonFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("LessonForm", model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        if (!await db.CourseModules.AnyAsync(module => module.Id == model.ModuleId && module.CourseId == model.CourseId))
        {
            return NotFound();
        }

        if (await db.Lessons.AnyAsync(lesson => lesson.CourseModuleId == model.ModuleId && lesson.Order == model.Order))
        {
            ModelState.AddModelError(nameof(model.Order), "Ja existe uma aula com esta ordem.");
            return View("LessonForm", model);
        }

        db.Lessons.Add(new Lesson
        {
            CourseModuleId = model.ModuleId,
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            VideoProvider = model.VideoProvider.Trim(),
            VideoExternalId = model.VideoExternalId.Trim(),
            DurationInMinutes = model.DurationInMinutes,
            Order = model.Order
        });

        await db.SaveChangesAsync();
        TempData["Success"] = "Aula criada com sucesso.";
        return RedirectToAction(nameof(Content), new { id = model.CourseId });
    }

    public async Task<IActionResult> EditLesson(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var lesson = await db.Lessons
            .AsNoTracking()
            .Include(item => item.CourseModule)
            .FirstOrDefaultAsync(item => item.Id == id);

        return lesson is null
            ? NotFound()
            : View("LessonForm", new LessonFormViewModel
            {
                Id = lesson.Id,
                CourseId = lesson.CourseModule.CourseId,
                ModuleId = lesson.CourseModuleId,
                Title = lesson.Title,
                Description = lesson.Description,
                VideoProvider = lesson.VideoProvider,
                VideoExternalId = lesson.VideoExternalId,
                DurationInMinutes = lesson.DurationInMinutes,
                Order = lesson.Order
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLesson(Guid id, LessonFormViewModel model)
    {
        if (id != model.Id || !ModelState.IsValid)
        {
            return View("LessonForm", model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var lesson = await db.Lessons
            .Include(item => item.CourseModule)
            .FirstOrDefaultAsync(item => item.Id == id && item.CourseModuleId == model.ModuleId);
        if (lesson is null || lesson.CourseModule.CourseId != model.CourseId)
        {
            return NotFound();
        }

        lesson.Title = model.Title.Trim();
        lesson.Description = model.Description.Trim();
        lesson.VideoProvider = model.VideoProvider.Trim();
        lesson.VideoExternalId = model.VideoExternalId.Trim();
        lesson.DurationInMinutes = model.DurationInMinutes;
        if (lesson.Order != model.Order &&
            await db.Lessons.AnyAsync(item => item.CourseModuleId == model.ModuleId && item.Order == model.Order && item.Id != id))
        {
            ModelState.AddModelError(nameof(model.Order), "Ja existe uma aula com esta ordem.");
            return View("LessonForm", model);
        }

        lesson.Order = model.Order;
        lesson.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Success"] = "Aula atualizada com sucesso.";
        return RedirectToAction(nameof(Content), new { id = model.CourseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveModule(Guid id, string direction)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var module = await db.CourseModules.FirstOrDefaultAsync(item => item.Id == id);
        if (module is null)
        {
            return NotFound();
        }

        await SwapModuleOrderAsync(db, module, direction == "up" ? -1 : 1);
        return RedirectToAction(nameof(Content), new { id = module.CourseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveLesson(Guid id, string direction)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var lesson = await db.Lessons.Include(item => item.CourseModule).FirstOrDefaultAsync(item => item.Id == id);
        if (lesson is null)
        {
            return NotFound();
        }

        await SwapLessonOrderAsync(db, lesson, direction == "up" ? -1 : 1);
        return RedirectToAction(nameof(Content), new { id = lesson.CourseModule.CourseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteModule(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var module = await db.CourseModules
            .Include(item => item.Lessons)
                .ThenInclude(lesson => lesson.ProgressEntries)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (module is null)
        {
            return NotFound();
        }

        if (module.Lessons.Any(lesson => lesson.ProgressEntries.Count > 0))
        {
            TempData["Error"] = "Modulo possui progresso de alunos e nao pode ser removido.";
            return RedirectToAction(nameof(Content), new { id = module.CourseId });
        }

        db.CourseModules.Remove(module);
        await db.SaveChangesAsync();
        TempData["Success"] = "Modulo removido com seguranca.";
        return RedirectToAction(nameof(Content), new { id = module.CourseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLesson(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var lesson = await db.Lessons
            .Include(item => item.CourseModule)
            .Include(item => item.ProgressEntries)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (lesson is null)
        {
            return NotFound();
        }

        if (lesson.ProgressEntries.Count > 0)
        {
            TempData["Error"] = "Aula possui progresso de alunos e nao pode ser removida.";
            return RedirectToAction(nameof(Content), new { id = lesson.CourseModule.CourseId });
        }

        var courseId = lesson.CourseModule.CourseId;
        db.Lessons.Remove(lesson);
        await db.SaveChangesAsync();
        TempData["Success"] = "Aula removida com seguranca.";
        return RedirectToAction(nameof(Content), new { id = courseId });
    }

    public async Task<IActionResult> Lesson(Guid id, Guid? studentId)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction(nameof(Index));
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var lessonData = await db.Lessons
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new LessonPlayerData
            {
                LessonId = item.Id,
                LessonTitle = item.Title,
                LessonDescription = item.Description,
                LessonOrder = item.Order,
                LessonDurationInMinutes = item.DurationInMinutes,
                VideoProvider = item.VideoProvider,
                VideoExternalId = item.VideoExternalId,
                ModuleId = item.CourseModule.Id,
                ModuleTitle = item.CourseModule.Title,
                ModuleDescription = item.CourseModule.Description,
                ModuleOrder = item.CourseModule.Order,
                CourseId = item.CourseModule.Course.Id,
                CourseTitle = item.CourseModule.Course.Title,
                CourseDescription = item.CourseModule.Course.Description,
                CourseIsActive = item.CourseModule.Course.IsActive,
                Modules = item.CourseModule.Course.Modules
                    .OrderBy(module => module.Order)
                    .Select(module => new LessonPlayerModuleData
                    {
                        Id = module.Id,
                        Title = module.Title,
                        Description = module.Description,
                        Order = module.Order,
                        Lessons = module.Lessons
                            .OrderBy(lesson => lesson.Order)
                            .Select(lesson => new LessonPlayerLessonData
                            {
                                Id = lesson.Id,
                                ModuleId = lesson.CourseModuleId,
                                Title = lesson.Title,
                                Description = lesson.Description,
                                VideoProvider = lesson.VideoProvider,
                                VideoExternalId = lesson.VideoExternalId,
                                DurationInMinutes = lesson.DurationInMinutes,
                                Order = lesson.Order
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
        if (lessonData is null)
        {
            return NotFound();
        }

        var students = await GetTenantStudentsAsync(db, tenantId.Value);
        var selectedStudentId = studentId.HasValue && students.Any(student => student.Id == studentId.Value)
            ? studentId
            : students.FirstOrDefault()?.Id;

        var orderedLessons = lessonData.Modules
            .OrderBy(module => module.Order)
            .SelectMany(module => module.Lessons.OrderBy(item => item.Order))
            .ToList();
        var lessonIds = orderedLessons.Select(item => item.Id).ToList();

        var progressItems = await db.StudentProgress
            .AsNoTracking()
            .Where(item =>
                lessonIds.Contains(item.LessonId) &&
                item.Student.TenantId == tenantId.Value)
            .Select(item => new LessonProgressData
            {
                LessonId = item.LessonId,
                StudentId = item.StudentId,
                WatchedPercentage = item.WatchedPercentage,
                CompletedAt = item.CompletedAt
            })
            .ToListAsync();

        var progressByLessonId = progressItems
            .GroupBy(item => item.LessonId)
            .ToDictionary(group => group.Key, group => new
            {
                Students = group.Select(item => item.StudentId).Distinct().Count(),
                AverageProgress = ClampPercent((int)Math.Round(group.Average(item => item.WatchedPercentage))),
                CompletionRate = ClampPercent((int)Math.Round(group.Count(item => item.CompletedAt.HasValue || item.WatchedPercentage >= 100) * 100m / group.Count()))
            });

        var selectedProgress = selectedStudentId.HasValue
            ? progressItems.FirstOrDefault(item => item.StudentId == selectedStudentId.Value && item.LessonId == id)
            : null;
        var completedLessonIds = selectedStudentId.HasValue
            ? progressItems
                .Where(item =>
                    item.StudentId == selectedStudentId.Value &&
                    (item.CompletedAt.HasValue || item.WatchedPercentage >= 100))
                .Select(item => item.LessonId)
                .ToHashSet()
            : [];

        var modules = lessonData.Modules
            .OrderBy(module => module.Order)
            .Select(module => new CourseModuleDetailsViewModel
            {
                Id = module.Id,
                Title = module.Title,
                Description = module.Description,
                Order = module.Order,
                Lessons = module.Lessons
                    .OrderBy(lesson => lesson.Order)
                    .Select(lesson =>
                    {
                        progressByLessonId.TryGetValue(lesson.Id, out var stats);
                        return new CourseLessonDetailsViewModel
                        {
                            Id = lesson.Id,
                            ModuleId = lesson.ModuleId,
                            Title = lesson.Title,
                            Description = lesson.Description,
                            VideoProvider = string.IsNullOrWhiteSpace(lesson.VideoProvider) ? "Estrutura pronta" : lesson.VideoProvider,
                            VideoExternalId = lesson.VideoExternalId,
                            DurationInMinutes = lesson.DurationInMinutes,
                            Order = lesson.Order,
                            Students = stats?.Students ?? 0,
                            AverageProgress = stats?.AverageProgress ?? 0,
                            CompletionRate = stats?.CompletionRate ?? 0
                        };
                    })
                    .ToList()
            })
            .ToList();

        var currentIndex = orderedLessons.FindIndex(item => item.Id == lessonData.LessonId);
        var duration = orderedLessons.Sum(item => item.DurationInMinutes);
        var remainingMinutes = orderedLessons
            .Where(item => !completedLessonIds.Contains(item.Id))
            .Sum(item => item.DurationInMinutes);
        var courseProgress = orderedLessons.Count == 0
            ? 0
            : ClampPercent((int)Math.Round(orderedLessons.Count(item => completedLessonIds.Contains(item.Id)) * 100m / orderedLessons.Count));
        var category = InferCategory(lessonData.CourseTitle);
        var teacher = InferTeacher(lessonData.CourseTitle);

        return View(new LessonPlayerViewModel
        {
            CourseId = lessonData.CourseId,
            CourseTitle = lessonData.CourseTitle,
            CourseCategory = category,
            CourseLevel = InferLevel(lessonData.Modules.Count),
            CourseTeacher = teacher,
            CourseWorkloadHours = Math.Max(1, (int)Math.Ceiling(duration / 60m)),
            LessonId = lessonData.LessonId,
            LessonTitle = lessonData.LessonTitle,
            LessonDescription = lessonData.LessonDescription,
            ModuleTitle = lessonData.ModuleTitle,
            VideoProvider = lessonData.VideoProvider,
            VideoExternalId = lessonData.VideoExternalId,
            DurationInMinutes = lessonData.LessonDurationInMinutes,
            StudentId = selectedStudentId,
            StudentName = students.FirstOrDefault(item => item.Id == selectedStudentId)?.Name,
            Progress = selectedProgress?.WatchedPercentage ?? 0,
            IsCompleted = selectedProgress?.CompletedAt.HasValue == true || selectedProgress?.WatchedPercentage >= 100,
            CourseProgress = courseProgress,
            PreviousLessonId = currentIndex > 0 ? orderedLessons[currentIndex - 1].Id : null,
            NextLessonId = currentIndex >= 0 && currentIndex < orderedLessons.Count - 1 ? orderedLessons[currentIndex + 1].Id : null,
            CompletedLessonIds = completedLessonIds,
            Students = students,
            Modules = modules,
            Objectives = BuildLessonObjectives(lessonData.LessonTitle),
            Materials = BuildLessonMaterials(lessonData.LessonTitle),
            Downloads = BuildLessonDownloads(),
            Quiz = BuildLessonQuiz(lessonData.LessonTitle),
            Discussions = BuildLessonDiscussions(lessonData.LessonTitle),
            Notes = BuildLessonNotes(lessonData.LessonDescription, remainingMinutes),
            Teacher = BuildLessonTeacher(teacher, category, lessonData.Modules.Count)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteLesson(Guid id, Guid studentId)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction(nameof(Index));
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var studentExists = await db.Students.AnyAsync(item => item.Id == studentId && item.TenantId == tenantId.Value && !item.IsDeleted);
        var lesson = await db.Lessons
            .Include(item => item.CourseModule)
                .ThenInclude(module => module.Course)
                    .ThenInclude(course => course.Modules)
                        .ThenInclude(module => module.Lessons)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (!studentExists || lesson is null)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        var progress = await db.StudentProgress.FirstOrDefaultAsync(item => item.StudentId == studentId && item.LessonId == id);
        if (progress is null)
        {
            db.StudentProgress.Add(new StudentProgress
            {
                StudentId = studentId,
                LessonId = id,
                WatchedPercentage = 100,
                CompletedAt = now,
                CreatedAt = now
            });
        }
        else
        {
            progress.WatchedPercentage = 100;
            progress.CompletedAt ??= now;
            progress.UpdatedAt = now;
        }

        await db.SaveChangesAsync();
        await IssueCertificateIfCourseCompletedAsync(db, lesson.CourseModule.Course, studentId, now);
        TempData["Success"] = "Aula concluida e progresso atualizado.";
        return RedirectToAction(nameof(Lesson), new { id, studentId });
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
        var completed = progress.Count(item => item.CompletedAt.HasValue || item.WatchedPercentage >= 100);

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
            AverageProgress = progress.Count == 0 ? 0 : ClampPercent((int)Math.Round(progress.Average(item => item.WatchedPercentage))),
            CompletionRate = progress.Count == 0 ? 0 : ClampPercent((int)Math.Round(completed * 100m / progress.Count)),
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
            CompletionRate = item.CompletionRate,
            Modules = course.Modules
                .OrderBy(module => module.Order)
                .Select(module => new CourseModuleDetailsViewModel
                {
                    Id = module.Id,
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
                                Id = lesson.Id,
                                ModuleId = lesson.CourseModuleId,
                                Title = lesson.Title,
                                Description = lesson.Description,
                                VideoProvider = string.IsNullOrWhiteSpace(lesson.VideoProvider) ? "Estrutura pronta" : lesson.VideoProvider,
                                VideoExternalId = lesson.VideoExternalId,
                                DurationInMinutes = lesson.DurationInMinutes,
                                Order = lesson.Order,
                                Students = progress.Select(entry => entry.StudentId).Distinct().Count(),
                                AverageProgress = progress.Count == 0 ? 0 : ClampPercent((int)Math.Round(progress.Average(entry => entry.WatchedPercentage))),
                                CompletionRate = progress.Count == 0 ? 0 : ClampPercent((int)Math.Round(progress.Count(entry => entry.CompletedAt.HasValue || entry.WatchedPercentage >= 100) * 100m / progress.Count))
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

    private static IReadOnlyList<string> BuildLessonObjectives(string lessonTitle)
    {
        return
        [
            $"Compreender o tema central de {lessonTitle}.",
            "Relacionar o conteudo com metas terapeuticas reais.",
            "Registrar uma acao pratica para aplicar antes da proxima aula."
        ];
    }

    private static IReadOnlyList<LessonMaterialViewModel> BuildLessonMaterials(string lessonTitle)
    {
        var code = NormalizeCode(lessonTitle);
        return
        [
            new() { Name = $"Guia pratico - {lessonTitle}", Type = "PDF", Size = "1.8 MB", Icon = "ph-file-pdf" },
            new() { Name = $"Mapa visual {code}", Type = "Slides", Size = "4.2 MB", Icon = "ph-presentation-chart" },
            new() { Name = "Checklist de aplicacao", Type = "Planilha", Size = "620 KB", Icon = "ph-table" }
        ];
    }

    private static IReadOnlyList<LessonDownloadViewModel> BuildLessonDownloads()
    {
        return
        [
            new() { Name = "Resumo executivo da aula", Format = "PDF", Size = "980 KB", Icon = "ph-file-pdf" },
            new() { Name = "Atividade orientada", Format = "DOCX", Size = "410 KB", Icon = "ph-file-doc" },
            new() { Name = "Pacote de apoio", Format = "ZIP", Size = "6.4 MB", Icon = "ph-file-zip" }
        ];
    }

    private static LessonQuizViewModel BuildLessonQuiz(string lessonTitle)
    {
        return new LessonQuizViewModel
        {
            Title = $"Verificacao rapida - {lessonTitle}",
            PassingScore = 70,
            Questions =
            [
                new()
                {
                    Text = "Qual e o primeiro passo recomendado apos assistir esta aula?",
                    Answers =
                    [
                        new() { Text = "Registrar uma acao pratica no plano pessoal.", IsCorrect = true },
                        new() { Text = "Pular para o certificado sem revisar o conteudo." },
                        new() { Text = "Ignorar as orientacoes do modulo." }
                    ]
                },
                new()
                {
                    Text = "Como o aluno deve usar os materiais complementares?",
                    Answers =
                    [
                        new() { Text = "Como apoio para aplicar o conteudo na rotina.", IsCorrect = true },
                        new() { Text = "Apenas como arquivo administrativo." },
                        new() { Text = "Somente depois do encerramento do curso." }
                    ]
                }
            ]
        };
    }

    private static IReadOnlyList<LessonDiscussionViewModel> BuildLessonDiscussions(string lessonTitle)
    {
        return
        [
            new() { Author = "Equipe terapeutica", Text = $"Quais situacoes reais voce conecta com {lessonTitle.ToLowerInvariant()}?", Likes = 12 },
            new() { Author = "Coordenacao pedagogica", Text = "Use este espaco para registrar duvidas que podem ser retomadas no grupo.", Likes = 8 }
        ];
    }

    private static IReadOnlyList<string> BuildLessonNotes(string lessonDescription, int remainingMinutes)
    {
        return
        [
            $"Ideia-chave: {lessonDescription}",
            $"Tempo estimado restante no curso: {remainingMinutes} minutos.",
            "Nota pessoal demonstrativa: transformar aprendizado em uma pequena acao observavel."
        ];
    }

    private static LessonTeacherViewModel BuildLessonTeacher(string teacher, string category, int courses)
    {
        return new LessonTeacherViewModel
        {
            Name = teacher,
            Specialty = category,
            Bio = "Profissional responsavel por conectar conteudo educacional, rotina terapeutica e plano de desenvolvimento individual.",
            Courses = Math.Max(1, courses)
        };
    }

    private static string NormalizeCode(string value)
    {
        return new string(value.ToLowerInvariant().Where(char.IsLetterOrDigit).Take(18).ToArray());
    }

    private sealed class LessonPlayerData
    {
        public Guid LessonId { get; set; }

        public string LessonTitle { get; set; } = string.Empty;

        public string LessonDescription { get; set; } = string.Empty;

        public int LessonOrder { get; set; }

        public int LessonDurationInMinutes { get; set; }

        public string VideoProvider { get; set; } = string.Empty;

        public string VideoExternalId { get; set; } = string.Empty;

        public Guid ModuleId { get; set; }

        public string ModuleTitle { get; set; } = string.Empty;

        public string ModuleDescription { get; set; } = string.Empty;

        public int ModuleOrder { get; set; }

        public Guid CourseId { get; set; }

        public string CourseTitle { get; set; } = string.Empty;

        public string CourseDescription { get; set; } = string.Empty;

        public bool CourseIsActive { get; set; }

        public List<LessonPlayerModuleData> Modules { get; set; } = [];
    }

    private sealed class LessonPlayerModuleData
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Order { get; set; }

        public List<LessonPlayerLessonData> Lessons { get; set; } = [];
    }

    private sealed class LessonPlayerLessonData
    {
        public Guid Id { get; set; }

        public Guid ModuleId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string VideoProvider { get; set; } = string.Empty;

        public string VideoExternalId { get; set; } = string.Empty;

        public int DurationInMinutes { get; set; }

        public int Order { get; set; }
    }

    private sealed class LessonProgressData
    {
        public Guid LessonId { get; set; }

        public Guid StudentId { get; set; }

        public int WatchedPercentage { get; set; }

        public DateTime? CompletedAt { get; set; }
    }

    private static string InferLevel(int modules) => modules switch
    {
        <= 1 => "Inicial",
        <= 3 => "Intermediário",
        _ => "Avançado"
    };
    private static int ClampPercent(int value) => Math.Clamp(value, 0, 100);

    private async Task<IActionResult> SetActiveAsync(Guid id, bool isActive, string message)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var course = await db.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        course.IsActive = isActive;
        course.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Success"] = message;
        return RedirectToAction(nameof(Index));
    }

    private async Task<Course?> LoadCourseDetailsAsync(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.Courses
            .AsNoTracking()
            .Include(course => course.Modules)
                .ThenInclude(module => module.Lessons)
                    .ThenInclude(lesson => lesson.ProgressEntries)
                        .ThenInclude(progress => progress.Student)
            .Include(course => course.Certificates)
                .ThenInclude(certificate => certificate.Student)
            .AsSplitQuery()
            .FirstOrDefaultAsync(course => course.Id == id);
    }

    private static async Task<IReadOnlyList<Renova.Web.Areas.EAD.ViewModels.Students.StudentOptionViewModel>> GetTenantStudentsAsync(
        AppDbContext db,
        Guid tenantId)
    {
        return await db.Students
            .AsNoTracking()
            .Include(student => student.Person)
            .Where(student => student.TenantId == tenantId && !student.IsDeleted)
            .OrderBy(student => student.Person != null ? student.Person.FullName : student.FullName)
            .Select(student => new Renova.Web.Areas.EAD.ViewModels.Students.StudentOptionViewModel
            {
                Id = student.Id,
                Name = student.Person != null ? student.Person.FullName : student.FullName
            })
            .ToListAsync();
    }

    private static async Task SwapModuleOrderAsync(AppDbContext db, CourseModule module, int delta)
    {
        var targetOrder = module.Order + delta;
        var target = await db.CourseModules.FirstOrDefaultAsync(item =>
            item.CourseId == module.CourseId &&
            item.Order == targetOrder);
        if (target is null)
        {
            return;
        }

        var originalOrder = module.Order;
        module.Order = -1;
        await db.SaveChangesAsync();
        target.Order = originalOrder;
        await db.SaveChangesAsync();
        module.Order = targetOrder;
        module.UpdatedAt = DateTime.UtcNow;
        target.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static async Task SwapLessonOrderAsync(AppDbContext db, Lesson lesson, int delta)
    {
        var targetOrder = lesson.Order + delta;
        var target = await db.Lessons.FirstOrDefaultAsync(item =>
            item.CourseModuleId == lesson.CourseModuleId &&
            item.Order == targetOrder);
        if (target is null)
        {
            return;
        }

        var originalOrder = lesson.Order;
        lesson.Order = -1;
        await db.SaveChangesAsync();
        target.Order = originalOrder;
        await db.SaveChangesAsync();
        lesson.Order = targetOrder;
        lesson.UpdatedAt = DateTime.UtcNow;
        target.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static async Task IssueCertificateIfCourseCompletedAsync(
        AppDbContext db,
        Course course,
        Guid studentId,
        DateTime issuedAt)
    {
        var lessonIds = course.Modules.SelectMany(module => module.Lessons).Select(lesson => lesson.Id).ToList();
        if (lessonIds.Count == 0)
        {
            return;
        }

        var completedLessonIds = await db.StudentProgress
            .Where(progress =>
                progress.StudentId == studentId &&
                lessonIds.Contains(progress.LessonId) &&
                (progress.CompletedAt.HasValue || progress.WatchedPercentage >= 100))
            .Select(progress => progress.LessonId)
            .Distinct()
            .ToListAsync();

        if (completedLessonIds.Count != lessonIds.Count ||
            await db.Certificates.AnyAsync(certificate => certificate.StudentId == studentId && certificate.CourseId == course.Id))
        {
            return;
        }

        db.Certificates.Add(new Certificate
        {
            StudentId = studentId,
            CourseId = course.Id,
            VerificationCode = $"RENOVA-{Guid.NewGuid():N}"[..32].ToUpperInvariant(),
            IssuedAt = issuedAt
        });

        await db.SaveChangesAsync();
    }
}
