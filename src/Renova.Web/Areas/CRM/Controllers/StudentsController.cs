using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.CRM.ViewModels.Students;
using Renova.Web.Services;
using Renova.Web.ViewModels;



namespace Renova.Web.Areas.CRM.Controllers;

[Area("CRM")]
public sealed class StudentsController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IPhotoService photoService) : Controller
{
    public async Task<IActionResult> Index(string? search, int? status, int page = 1)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        IQueryable<Student> query = db.Students
            .AsNoTracking()
            .Include(student => student.Person);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(student =>
                student.FullName.ToLower().Contains(term) ||
                (student.Person != null && student.Person.FullName.ToLower().Contains(term)) ||
                student.CPF.ToLower().Contains(term) ||
                (student.Person != null && student.Person.Cpf != null && student.Person.Cpf.ToLower().Contains(term)) ||
                (student.Email != null && student.Email.ToLower().Contains(term)) ||
                (student.Person != null && student.Person.Email != null && student.Person.Email.ToLower().Contains(term)));
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

        const int pageSize = 10;
        page = Math.Max(1, page);
        var totalItems = await query.CountAsync();

        var students = await query
            .OrderBy(student => student.Person != null ? student.Person.FullName : student.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(new PagedResult<Student>
        {
            Items = students,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students
            .AsNoTracking()
            .Include(item => item.Person)
            .Include(item => item.FamilyMembers)
            .Include(item => item.Appointments)
                .ThenInclude(appointment => appointment.Professional)
            .Include(item => item.Subscriptions)
            .Include(item => item.ProgressEntries)
                .ThenInclude(progress => progress.Lesson)
            .Include(item => item.MedicalEvolutions)
                .ThenInclude(evolution => evolution.Professional)
            .FirstOrDefaultAsync(item => item.Id == id);

        return student is null ? NotFound() : View(student);
    }

    public IActionResult Create()
    {
        return View(new StudentFormViewModel
        {
            BirthDate = DateTime.Today.AddYears(-30),
            AdmissionDate = DateTime.Today,
            Status = StudentStatuses.InTreatment
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();

        string? photoPath;

        try
        {
            photoPath = await photoService.SavePhotoAsync(model.Photo, "students");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Photo), ex.Message);
            return View(model);
        }

        var student = new Student
        {
            FullName = model.FullName.Trim(),
            CPF = model.CPF.Trim(),
            BirthDate = DateTime.SpecifyKind(model.BirthDate.Date, DateTimeKind.Utc),
            Phone = model.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim(),
            Status = model.Status,
            AdmissionDate = DateTime.SpecifyKind(model.AdmissionDate.Date, DateTimeKind.Utc),
            PhotoPath = photoPath,
            CreatedAt = DateTime.UtcNow
        };

        db.Students.Add(student);

        if (!await TrySaveAsync(db, "Acolhido cadastrado com sucesso."))
        {
            photoService.DeletePhoto(photoPath);
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

        var oldPhotoPath = student.PhotoPath;

        try
        {
            student.PhotoPath = await photoService.SavePhotoAsync(model.Photo, "students", student.PhotoPath);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Photo), ex.Message);
            model.PhotoPath = oldPhotoPath;
            return View(model);
        }

        student.FullName = model.FullName.Trim();
        student.CPF = model.CPF.Trim();
        student.BirthDate = DateTime.SpecifyKind(model.BirthDate.Date, DateTimeKind.Utc);
        student.Phone = model.Phone.Trim();
        student.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        student.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
        student.Status = model.Status;
        student.AdmissionDate = DateTime.SpecifyKind(model.AdmissionDate.Date, DateTimeKind.Utc);
        student.UpdatedAt = DateTime.UtcNow;

        if (!await TrySaveAsync(db, "Acolhido atualizado com sucesso."))
        {
            return View(ToForm(student));
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students.FindAsync(id);

        if (student is null)
        {
            return NotFound();
        }

        try
        {
            var photoPath = student.PhotoPath;

            db.Students.Remove(student);
            await db.SaveChangesAsync();

            photoService.DeletePhoto(photoPath);

            TempData["Success"] = "Acolhido excluído com sucesso.";
        }
        catch (DbUpdateException)
        {
            student.Status = StudentStatuses.Inactive;
            student.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            TempData["Success"] = "Acolhido possui vínculos e foi inativado com segurança.";
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Profile()
    {
        return View();
    }

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

    private static StudentFormViewModel ToForm(Student student)
    {
        return new StudentFormViewModel
        {
            Id = student.Id,
            FullName = student.FullName,
            CPF = student.CPF,
            BirthDate = student.BirthDate,
            Phone = student.Phone,
            Email = student.Email,
            Address = student.Address,
            Status = student.Status,
            AdmissionDate = student.AdmissionDate,
            PhotoPath = student.PhotoPath
        };
    }
}
