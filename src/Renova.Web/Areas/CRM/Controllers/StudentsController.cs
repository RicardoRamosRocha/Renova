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
    IPhotoService photoService,
    ICurrentTenantService currentTenantService) : Controller
{
    private const string MissingTenantMessage = "Não foi possível identificar a instituição atual. Entre novamente ou contate o administrador.";

    public async Task<IActionResult> Index(
        string? search,
        int? status,
        DateTime? admissionFrom,
        DateTime? admissionTo,
        int page = 1)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return View(new StudentIndexViewModel
            {
                Students = new PagedResult<StudentIndexItemViewModel>
                {
                    Items = [],
                    Page = 1,
                    PageSize = 10,
                    TotalItems = 0
                }
            });
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        IQueryable<Student> query = db.Students
            .AsNoTracking()
            .Include(student => student.Person)
            .Where(student => student.TenantId == tenantId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(student =>
                student.FullName.ToLower().Contains(term) ||
                (student.Person != null && student.Person.FullName.ToLower().Contains(term)) ||
                student.CPF.ToLower().Contains(term) ||
                (student.Person != null && student.Person.Cpf != null && student.Person.Cpf.ToLower().Contains(term)) ||
                student.Phone.ToLower().Contains(term) ||
                (student.Person != null && student.Person.Phone != null && student.Person.Phone.ToLower().Contains(term)) ||
                (student.Email != null && student.Email.ToLower().Contains(term)) ||
                (student.Person != null && student.Person.Email != null && student.Person.Email.ToLower().Contains(term)));
        }

        if (status.HasValue)
        {
            query = query.Where(student => student.Status == status.Value);
        }

        if (admissionFrom.HasValue)
        {
            var from = DateTime.SpecifyKind(admissionFrom.Value.Date, DateTimeKind.Utc);
            query = query.Where(student => student.AdmissionDate >= from);
        }

        if (admissionTo.HasValue)
        {
            var to = DateTime.SpecifyKind(admissionTo.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(student => student.AdmissionDate < to);
        }

        var metricsQuery = db.Students
            .AsNoTracking()
            .Where(student => student.TenantId == tenantId.Value);

        const int pageSize = 10;
        page = Math.Max(1, page);
        var totalItems = await query.CountAsync();

        var students = await query
            .OrderBy(student => student.Person != null ? student.Person.FullName : student.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(student => new StudentIndexItemViewModel
            {
                Id = student.Id,
                Name = student.Person != null ? student.Person.FullName : student.FullName,
                Cpf = student.Person != null && student.Person.Cpf != null ? student.Person.Cpf : student.CPF,
                Phone = student.Person != null && student.Person.Phone != null ? student.Person.Phone : student.Phone,
                Email = student.Person != null && student.Person.Email != null ? student.Person.Email : student.Email,
                PhotoUrl = student.Person != null && student.Person.PhotoUrl != null ? student.Person.PhotoUrl : student.PhotoPath,
                AdmissionDate = student.AdmissionDate,
                Status = student.Status,
                ResponsibleProfessional = student.Admissions
                    .Where(admission => admission.AdmissionStatus == AdmissionStatus.Active)
                    .OrderByDescending(admission => admission.AdmissionDate)
                    .Select(admission => admission.ResponsibleProfessional)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return View(new StudentIndexViewModel
        {
            Search = search,
            Status = status,
            AdmissionFrom = admissionFrom,
            AdmissionTo = admissionTo,
            Total = await metricsQuery.CountAsync(),
            Active = await metricsQuery.CountAsync(student => student.Status != StudentStatuses.Inactive),
            InTreatment = await metricsQuery.CountAsync(student => student.Status == StudentStatuses.InTreatment),
            DischargePlanned = await metricsQuery.CountAsync(student => student.Status == StudentStatuses.DischargePlanned),
            Archived = await metricsQuery.CountAsync(student => student.Status == StudentStatuses.Inactive),
            Students = new PagedResult<StudentIndexItemViewModel>
            {
                Items = students,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
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
        var student = await db.Students
            .AsNoTracking()
            .Include(item => item.Tenant)
            .Include(item => item.Person)
            .Include(item => item.Admissions)
            .Include(item => item.FamilyMembers)
                .ThenInclude(member => member.Person)
            .Include(item => item.Appointments)
                .ThenInclude(appointment => appointment.Professional)
            .Include(item => item.Subscriptions)
            .Include(item => item.ProgressEntries)
                .ThenInclude(progress => progress.Lesson)
            .Include(item => item.MedicalEvolutions)
                .ThenInclude(evolution => evolution.Professional)
            .FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);

        return student is null ? NotFound() : View(ToDetails(student));
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
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            ModelState.AddModelError(string.Empty, MissingTenantMessage);
            return View(model);
        }

        string? photoPath;

        try
        {
            photoPath = model.RemovePhoto ? null : await photoService.SavePhotoAsync(model.Photo, "students");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Photo), ex.Message);
            return View(model);
        }

        var timestamp = DateTime.UtcNow;
        if (await HasDuplicateCpfAsync(db, tenantId.Value, model.CPF))
        {
            ModelState.AddModelError(nameof(model.CPF), "Já existe um acolhido ou pessoa com este CPF nesta instituição.");
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
            TenantId = tenantId.Value,
            CreatedAt = timestamp
        };

        student.SyncPersonFromLegacyFields(timestamp);

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
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction(nameof(Index));
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students
            .Include(item => item.Person)
            .FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);

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
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            ModelState.AddModelError(string.Empty, MissingTenantMessage);
            return View(model);
        }

        var student = await db.Students
            .Include(item => item.Person)
            .FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);

        if (student is null)
        {
            return NotFound();
        }

        var oldPhotoPath = student.DisplayPhotoUrl;

        try
        {
            if (model.RemovePhoto)
            {
                photoService.DeletePhoto(oldPhotoPath);
                student.PhotoPath = null;
            }
            else
            {
                student.PhotoPath = await photoService.SavePhotoAsync(model.Photo, "students", oldPhotoPath);
            }
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Photo), ex.Message);
            model.PhotoPath = oldPhotoPath;
            return View(model);
        }

        var timestamp = DateTime.UtcNow;
        if (await HasDuplicateCpfAsync(db, tenantId.Value, model.CPF, student.Id, student.PersonId))
        {
            ModelState.AddModelError(nameof(model.CPF), "Já existe um acolhido ou pessoa com este CPF nesta instituição.");
            model.PhotoPath = student.DisplayPhotoUrl;
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
        student.UpdatedAt = timestamp;
        student.SyncPersonFromLegacyFields(timestamp, markPersonAsUpdated: true);

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
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction(nameof(Index));
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students.FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);

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
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction(nameof(Index));
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students.FirstOrDefaultAsync(item => item.Id == id && item.TenantId == tenantId.Value);

        if (student is null)
        {
            return NotFound();
        }

        student.Status = StudentStatuses.Inactive;
        student.IsDeleted = true;
        student.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        TempData["Success"] = "Acolhido arquivado com segurança.";

        return RedirectToAction(nameof(Index));
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
            FullName = student.DisplayName,
            CPF = student.DisplayCpf,
            BirthDate = student.DisplayBirthDate,
            Phone = student.DisplayPhone,
            Email = student.DisplayEmail,
            Address = student.Address,
            Status = student.Status,
            AdmissionDate = student.AdmissionDate,
            PhotoPath = student.DisplayPhotoUrl
        };
    }

    private static StudentDetailsViewModel ToDetails(Student student)
    {
        var today = DateTime.Today;
        var birthDate = student.DisplayBirthDate.Date;
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age))
        {
            age--;
        }

        var admissions = student.Admissions
            .OrderByDescending(item => item.AdmissionDate)
            .Select(item => new StudentAdmissionSummaryViewModel
            {
                Id = item.Id,
                AdmissionDate = item.AdmissionDate,
                ExpectedDischargeDate = item.ExpectedDischargeDate,
                DischargeDate = item.DischargeDate,
                Status = AdmissionStatusLabel(item.AdmissionStatus),
                ResponsibleProfessional = item.ResponsibleProfessional
            })
            .ToList();

        var family = student.FamilyMembers
            .OrderByDescending(item => item.IsResponsible)
            .ThenBy(item => item.DisplayName)
            .Select(item => new StudentFamilySummaryViewModel
            {
                Id = item.Id,
                Name = item.DisplayName,
                Relationship = item.Relationship,
                Phone = item.DisplayPhone,
                Email = item.DisplayEmail,
                PhotoUrl = item.DisplayPhotoUrl,
                IsResponsible = item.IsResponsible,
                CanAccessPortal = item.CanAccessPortal
            })
            .ToList();

        var appointments = student.Appointments
            .Where(item => item.ScheduledAt.Date >= today)
            .OrderBy(item => item.ScheduledAt)
            .Take(3)
            .Select(item => new StudentAppointmentSummaryViewModel
            {
                ScheduledAt = item.ScheduledAt,
                ProfessionalName = item.Professional?.FullName ?? "Profissional a definir"
            })
            .ToList();

        var evolutions = student.MedicalEvolutions
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.CreatedAt)
            .Take(3)
            .Select(item => new StudentMedicalEvolutionSummaryViewModel
            {
                CreatedAt = item.CreatedAt,
                ProfessionalName = item.Professional.FullName,
                Description = item.Description
            })
            .ToList();

        var timeline = new List<TimelineItemViewModel>
        {
            new()
            {
                Icon = "ph-user-plus",
                Title = "Cadastro criado",
                Description = "Acolhido cadastrado no CRM.",
                OccurredAt = student.CreatedAt,
                Category = "Cadastro"
            }
        };

        timeline.AddRange(student.Admissions.Select(item => new TimelineItemViewModel
        {
            Icon = item.AdmissionStatus == AdmissionStatus.Active ? "ph-door-open" : "ph-flag",
            Title = AdmissionStatusLabel(item.AdmissionStatus),
            Description = item.AdmissionReason ?? item.DischargeReason,
            OccurredAt = item.DischargeDate ?? item.AdmissionDate,
            Category = "Admissão",
            Responsible = item.ResponsibleProfessional
        }));

        timeline.AddRange(evolutions.Select(item => new TimelineItemViewModel
        {
            Icon = "ph-heartbeat",
            Title = "Evolução registrada",
            Description = item.Description,
            OccurredAt = item.CreatedAt,
            Category = "Prontuário",
            Responsible = item.ProfessionalName
        }));

        timeline.AddRange(student.Appointments.Select(item => new TimelineItemViewModel
        {
            Icon = "ph-calendar-check",
            Title = "Atendimento agendado",
            Description = item.Notes,
            OccurredAt = item.ScheduledAt,
            Category = "Agenda",
            Responsible = item.Professional?.FullName
        }));

        return new StudentDetailsViewModel
        {
            Id = student.Id,
            Name = student.DisplayName,
            Cpf = student.DisplayCpf,
            Phone = student.DisplayPhone,
            Email = student.DisplayEmail,
            Address = student.Address,
            PhotoUrl = student.DisplayPhotoUrl,
            BirthDate = student.DisplayBirthDate,
            AdmissionDate = student.AdmissionDate,
            Status = student.Status,
            TenantName = student.Tenant.Name,
            ResponsibleProfessional = admissions.FirstOrDefault(item => item.Status == AdmissionStatusLabel(AdmissionStatus.Active))?.ResponsibleProfessional,
            Age = Math.Max(0, age),
            TreatmentDays = Math.Max(0, (today - student.AdmissionDate.Date).Days),
            Admissions = admissions,
            FamilyMembers = family,
            Appointments = appointments,
            MedicalEvolutions = evolutions,
            Timeline = timeline
                .OrderByDescending(item => item.OccurredAt)
                .Take(12)
                .ToList()
        };
    }

    private static async Task<bool> HasDuplicateCpfAsync(
        AppDbContext db,
        Guid tenantId,
        string cpf,
        Guid? currentStudentId = null,
        Guid? currentPersonId = null)
    {
        var normalizedCpf = cpf.Trim();

        var duplicateStudent = await db.Students.AnyAsync(student =>
            student.TenantId == tenantId &&
            student.Id != currentStudentId &&
            student.CPF == normalizedCpf);

        if (duplicateStudent)
        {
            return true;
        }

        return await db.People.AnyAsync(person =>
            person.TenantId == tenantId &&
            person.Id != currentPersonId &&
            person.Cpf == normalizedCpf);
    }

    private static string AdmissionStatusLabel(AdmissionStatus status) => status switch
    {
        AdmissionStatus.Active => "Ativa",
        AdmissionStatus.Planned => "Planejada",
        AdmissionStatus.Discharged => "Alta",
        AdmissionStatus.Cancelled => "Cancelada",
        AdmissionStatus.Transferred => "Transferida",
        _ => "Admissão"
    };
}
