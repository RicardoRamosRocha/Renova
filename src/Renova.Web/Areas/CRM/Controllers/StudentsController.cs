using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.CRM.ViewModels.Students;

namespace Renova.Web.Areas.CRM.Controllers;

[Area("CRM")]
public sealed class StudentsController(IDbContextFactory<AppDbContext> dbContextFactory) : Controller
{
    public async Task<IActionResult> Index(string? search, int? status)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var query = db.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(student =>
                student.FullName.ToLower().Contains(term) ||
                student.CPF.ToLower().Contains(term) ||
                (student.Email != null && student.Email.ToLower().Contains(term)));
        }

        if (status.HasValue)
        {
            query = query.Where(student => student.Status == status.Value);
        }

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.Statuses = StudentStatuses.Labels;
        ViewBag.Total = await db.Students.CountAsync();
        ViewBag.Active = await db.Students.CountAsync(student => student.Status != StudentStatuses.Inactive);
        ViewBag.InTreatment = await db.Students.CountAsync(student => student.Status == StudentStatuses.InTreatment);
        ViewBag.DischargePlanned = await db.Students.CountAsync(student => student.Status == StudentStatuses.DischargePlanned);

        var students = await query
            .OrderBy(student => student.FullName)
            .ToListAsync();

        return View(students);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return student is null ? NotFound() : View(student);
    }

    public IActionResult Create() => View(new StudentFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        db.Students.Add(new Student
        {
            FullName = model.FullName.Trim(),
            CPF = model.CPF.Trim(),
            BirthDate = DateTime.SpecifyKind(model.BirthDate.Date, DateTimeKind.Utc),
            Phone = model.Phone.Trim(),
            Email = model.Email?.Trim(),
            Address = model.Address?.Trim(),
            Status = model.Status,
            AdmissionDate = DateTime.SpecifyKind(model.AdmissionDate.Date, DateTimeKind.Utc)
        });

        if (!await TrySaveAsync(db, "Acolhido cadastrado com sucesso."))
        {
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students.FindAsync(id);
        return student is null ? NotFound() : View(ToForm(student));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, StudentFormViewModel model)
    {
        if (id != model.Id || !ModelState.IsValid)
        {
            return View(model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students.FindAsync(id);
        if (student is null)
        {
            return NotFound();
        }

        student.FullName = model.FullName.Trim();
        student.CPF = model.CPF.Trim();
        student.BirthDate = DateTime.SpecifyKind(model.BirthDate.Date, DateTimeKind.Utc);
        student.Phone = model.Phone.Trim();
        student.Email = model.Email?.Trim();
        student.Address = model.Address?.Trim();
        student.Status = model.Status;
        student.AdmissionDate = DateTime.SpecifyKind(model.AdmissionDate.Date, DateTimeKind.Utc);
        student.UpdatedAt = DateTime.UtcNow;

        if (!await TrySaveAsync(db, "Acolhido atualizado com sucesso."))
        {
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inactivate(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students.FindAsync(id);
        if (student is null)
        {
            return NotFound();
        }

        student.Status = StudentStatuses.Inactive;
        student.UpdatedAt = DateTime.UtcNow;
        await TrySaveAsync(db, "Acolhido inativado com sucesso.");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Profile() => View();

    private async Task<bool> TrySaveAsync(AppDbContext db, string successMessage)
    {
        try
        {
            await db.SaveChangesAsync();
            TempData["Success"] = successMessage;
            return true;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Não foi possível salvar. Verifique se CPF ou dados únicos já existem.");
            return false;
        }
    }

    private static StudentFormViewModel ToForm(Student student) => new()
    {
        Id = student.Id,
        FullName = student.FullName,
        CPF = student.CPF,
        BirthDate = student.BirthDate,
        Phone = student.Phone,
        Email = student.Email,
        Address = student.Address,
        Status = student.Status,
        AdmissionDate = student.AdmissionDate
    };
}
