using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.EAD.ViewModels.Courses;

namespace Renova.Web.Areas.EAD.Controllers;

[Area("EAD")]
public sealed class CoursesController(IDbContextFactory<AppDbContext> dbContextFactory) : Controller
{
    public async Task<IActionResult> Index(string? search, bool? active)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var query = db.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(course =>
                course.Title.ToLower().Contains(term) ||
                course.Description.ToLower().Contains(term));
        }

        if (active.HasValue)
        {
            query = query.Where(course => course.IsActive == active.Value);
        }

        ViewBag.Search = search;
        ViewBag.Active = active;
        ViewBag.Total = await db.Courses.CountAsync();
        ViewBag.Published = await db.Courses.CountAsync(course => course.IsActive);
        ViewBag.Archived = await db.Courses.CountAsync(course => !course.IsActive);
        ViewBag.CreatedThisMonth = await db.Courses.CountAsync(course => course.CreatedAt >= DateTime.UtcNow.AddDays(-30));

        return View(await query.OrderBy(course => course.Title).ToListAsync());
    }

    public async Task<IActionResult> Details(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var course = await db.Courses.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return course is null ? NotFound() : View(course);
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

    private static CourseFormViewModel ToForm(Course course) => new()
    {
        Id = course.Id,
        Title = course.Title,
        Description = course.Description,
        IsActive = course.IsActive,
        Status = course.IsActive ? "Publicado" : "Arquivado"
    };
}
