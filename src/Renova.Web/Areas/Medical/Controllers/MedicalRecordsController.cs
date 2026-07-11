using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;
using Renova.Web.Areas.CRM.ViewModels.Students;
using Renova.Web.Areas.Medical.ViewModels.MedicalRecords;
using Renova.Web.Services;

namespace Renova.Web.Areas.Medical.Controllers;

[Area("Medical")]
public sealed class MedicalRecordsController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    ICurrentTenantService currentTenantService,
    IWebHostEnvironment environment) : Controller
{
    private const string MissingTenantMessage = "Não foi possível identificar a instituição atual. Entre novamente ou contate o administrador.";
    private const long MaxDocumentSize = 10 * 1024 * 1024;
    private static readonly string[] AllowedDocumentExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".webp", ".doc", ".docx"];

    public async Task<IActionResult> Index(Guid? studentId)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return View(new MedicalRecordViewModel());
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();

        var students = await db.Students
            .AsNoTracking()
            .Include(item => item.Person)
            .Where(item => item.TenantId == tenantId.Value)
            .OrderBy(item => item.Person != null ? item.Person.FullName : item.FullName)
            .Select(item => new MedicalRecordStudentOptionViewModel
            {
                Id = item.Id,
                Name = item.Person != null ? item.Person.FullName : item.FullName,
                Status = StudentStatuses.Label(item.Status)
            })
            .ToListAsync();

        var selectedStudentId = studentId ?? students.FirstOrDefault()?.Id;
        if (!selectedStudentId.HasValue)
        {
            return View(new MedicalRecordViewModel { Students = students });
        }

        var student = await db.Students
            .AsNoTracking()
            .Include(item => item.Tenant)
            .Include(item => item.Person)
                .ThenInclude(person => person!.Documents)
            .Include(item => item.MedicalRecord)
            .Include(item => item.MedicalEvolutions)
                .ThenInclude(evolution => evolution.Professional)
            .Include(item => item.Admissions)
            .Include(item => item.Appointments)
                .ThenInclude(appointment => appointment.Professional)
            .Include(item => item.ProgressEntries)
                .ThenInclude(progress => progress.Lesson)
            .Include(item => item.Payments)
            .FirstOrDefaultAsync(item => item.Id == selectedStudentId.Value && item.TenantId == tenantId.Value);

        if (student is null)
        {
            return NotFound();
        }

        var professionals = await db.Professionals
            .AsNoTracking()
            .Where(item => item.TenantId == tenantId.Value && item.IsActive)
            .OrderBy(item => item.Person != null ? item.Person.FullName : item.FullName)
            .Select(item => new SelectListItem
            {
                Value = item.Id.ToString(),
                Text = item.Person != null ? item.Person.FullName : item.FullName
            })
            .ToListAsync();

        return View(ToViewModel(student, students, professionals));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvolution(MedicalEvolutionFormViewModel model)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction(nameof(Index), new { studentId = model.StudentId });
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Informe profissional e descrição para registrar a evolução.";
            return RedirectToAction(nameof(Index), new { studentId = model.StudentId });
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var hasStudent = await db.Students.AnyAsync(item => item.Id == model.StudentId && item.TenantId == tenantId.Value);
        var hasProfessional = await db.Professionals.AnyAsync(item => item.Id == model.ProfessionalId && item.TenantId == tenantId.Value && item.IsActive);

        if (!hasStudent || !hasProfessional)
        {
            return NotFound();
        }

        db.MedicalEvolutions.Add(new MedicalEvolution
        {
            StudentId = model.StudentId,
            ProfessionalId = model.ProfessionalId,
            Description = model.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        TempData["Success"] = "Evolução registrada com assinatura profissional.";
        return RedirectToAction(nameof(Index), new { studentId = model.StudentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(MedicalDocumentUploadViewModel model)
    {
        var tenantId = await currentTenantService.GetTenantIdAsync();
        if (!tenantId.HasValue)
        {
            TempData["Error"] = MissingTenantMessage;
            return RedirectToAction(nameof(Index), new { studentId = model.StudentId });
        }

        if (model.File is null || model.File.Length == 0)
        {
            TempData["Error"] = "Selecione um documento para enviar.";
            return RedirectToAction(nameof(Index), new { studentId = model.StudentId });
        }

        if (model.File.Length > MaxDocumentSize)
        {
            TempData["Error"] = "O documento deve ter no máximo 10 MB.";
            return RedirectToAction(nameof(Index), new { studentId = model.StudentId });
        }

        var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
        if (!AllowedDocumentExtensions.Contains(extension))
        {
            TempData["Error"] = "Formato inválido. Use PDF, imagem, DOC ou DOCX.";
            return RedirectToAction(nameof(Index), new { studentId = model.StudentId });
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var student = await db.Students
            .Include(item => item.Person)
            .FirstOrDefaultAsync(item => item.Id == model.StudentId && item.TenantId == tenantId.Value);

        if (student?.PersonId is null || student.Person is null)
        {
            TempData["Error"] = "O acolhido precisa ter pessoa vinculada para receber documentos.";
            return RedirectToAction(nameof(Index), new { studentId = model.StudentId });
        }

        var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", "medical-documents");
        Directory.CreateDirectory(uploadsFolder);

        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, safeFileName);

        await using (var stream = new FileStream(filePath, FileMode.CreateNew))
        {
            await model.File.CopyToAsync(stream);
        }

        db.Documents.Add(new Document
        {
            TenantId = tenantId.Value,
            PersonId = student.PersonId.Value,
            Title = model.Title.Trim(),
            FileName = $"/uploads/medical-documents/{safeFileName}",
            ContentType = string.IsNullOrWhiteSpace(model.File.ContentType) ? "application/octet-stream" : model.File.ContentType,
            Size = model.File.Length,
            UploadDate = DateTime.UtcNow,
            UploadedBy = User.Identity?.Name,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        TempData["Success"] = "Documento anexado ao prontuário.";
        return RedirectToAction(nameof(Index), new { studentId = model.StudentId });
    }

    private static MedicalRecordViewModel ToViewModel(
        Student student,
        IReadOnlyList<MedicalRecordStudentOptionViewModel> students,
        IReadOnlyList<SelectListItem> professionals)
    {
        var today = DateTime.Today;
        var birthDate = student.DisplayBirthDate.Date;
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age))
        {
            age--;
        }

        var activeAdmission = student.Admissions
            .Where(item => item.AdmissionStatus == AdmissionStatus.Active)
            .OrderByDescending(item => item.AdmissionDate)
            .FirstOrDefault();

        var evolutions = student.MedicalEvolutions
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new MedicalEvolutionItemViewModel
            {
                CreatedAt = item.CreatedAt,
                ProfessionalName = item.Professional.DisplayName,
                EvolutionType = EvolutionTypeLabel(item.Professional.ProfessionalType),
                Description = item.Description,
                Signature = $"{item.Professional.DisplayName} · {item.Professional.RegistrationNumber}"
            })
            .ToList();

        var timeline = new List<MedicalTimelineItemViewModel>();

        timeline.AddRange(student.Admissions.Select(item => new MedicalTimelineItemViewModel
        {
            Icon = "ph-door-open",
            Title = item.AdmissionStatus == AdmissionStatus.Active ? "Admissão ativa" : $"Admissão {item.AdmissionStatus}",
            Description = item.AdmissionReason ?? item.DischargeReason,
            OccurredAt = item.DischargeDate ?? item.AdmissionDate,
            Category = "Admissão",
            Responsible = item.ResponsibleProfessional
        }));

        timeline.AddRange(evolutions.Select(item => new MedicalTimelineItemViewModel
        {
            Icon = "ph-notebook",
            Title = item.EvolutionType,
            Description = item.Description,
            OccurredAt = item.CreatedAt,
            Category = "Evolução",
            Responsible = item.ProfessionalName
        }));

        timeline.AddRange(student.Appointments.Select(item => new MedicalTimelineItemViewModel
        {
            Icon = "ph-calendar-check",
            Title = "Consulta agendada",
            Description = item.Notes,
            OccurredAt = item.ScheduledAt,
            Category = "Agenda",
            Responsible = item.Professional?.DisplayName
        }));

        timeline.AddRange(student.ProgressEntries
            .Where(item => item.CompletedAt.HasValue)
            .Select(item => new MedicalTimelineItemViewModel
            {
                Icon = "ph-graduation-cap",
                Title = "Curso concluído",
                Description = item.Lesson.Title,
                OccurredAt = item.CompletedAt!.Value,
                Category = "EAD"
            }));

        timeline.AddRange(student.Payments.Select(item => new MedicalTimelineItemViewModel
        {
            Icon = "ph-currency-circle-dollar",
            Title = "Movimento financeiro",
            Description = $"Pagamento {item.Status} · {item.Amount:C}",
            OccurredAt = item.PaidAt ?? item.CreatedAt,
            Category = "Financeiro"
        }));

        var documents = student.Person?.Documents
            .OrderByDescending(item => item.UploadDate)
            .Select(item => new MedicalDocumentItemViewModel
            {
                Title = item.Title,
                FileName = item.FileName,
                ContentType = item.ContentType,
                Size = item.Size,
                UploadDate = item.UploadDate,
                UploadedBy = item.UploadedBy
            })
            .ToList() ?? [];

        return new MedicalRecordViewModel
        {
            StudentId = student.Id,
            StudentName = student.DisplayName,
            Status = StudentStatuses.Label(student.Status),
            PhotoUrl = student.DisplayPhotoUrl,
            Age = Math.Max(0, age),
            CareDays = Math.Max(0, (today - student.AdmissionDate.Date).Days),
            ResponsibleProfessional = activeAdmission?.ResponsibleProfessional,
            Anamnesis = student.MedicalRecord?.Anamnesis,
            ClinicalNotes = student.MedicalRecord?.ClinicalNotes,
            Students = students,
            Evolutions = evolutions,
            Timeline = timeline
                .OrderByDescending(item => item.OccurredAt)
                .Take(20)
                .ToList(),
            Documents = documents,
            TherapeuticPlan = BuildTherapeuticPlan(activeAdmission, evolutions),
            Medications = [],
            Professionals = professionals,
            EvolutionForm = new MedicalEvolutionFormViewModel { StudentId = student.Id },
            DocumentUpload = new MedicalDocumentUploadViewModel { StudentId = student.Id }
        };
    }

    private static IReadOnlyList<TherapeuticPlanItemViewModel> BuildTherapeuticPlan(
        Admission? activeAdmission,
        IReadOnlyList<MedicalEvolutionItemViewModel> evolutions)
    {
        if (activeAdmission is null && evolutions.Count == 0)
        {
            return [];
        }

        return
        [
            new TherapeuticPlanItemViewModel
            {
                Objective = "Estabilização e adesão ao cuidado",
                Goal = activeAdmission?.ExpectedDischargeDate is null
                    ? "Definir metas clínicas na próxima evolução."
                    : $"Preparar alta até {activeAdmission.ExpectedDischargeDate:dd/MM/yyyy}.",
                Intervention = "Atendimentos multiprofissionais, rotina terapêutica e acompanhamento familiar.",
                Periodicity = "Semanal",
                Responsible = activeAdmission?.ResponsibleProfessional ?? evolutions.FirstOrDefault()?.ProfessionalName ?? "Equipe técnica",
                Status = activeAdmission is null ? "Em estruturação" : "Ativo",
                Conclusion = activeAdmission?.DischargeDate is null ? null : $"Concluído em {activeAdmission.DischargeDate:dd/MM/yyyy}"
            }
        ];
    }

    private static string EvolutionTypeLabel(ProfessionalType? type) => type switch
    {
        ProfessionalType.Doctor or ProfessionalType.Psychiatrist => "Médica",
        ProfessionalType.Psychologist or ProfessionalType.Therapist => "Psicológica",
        ProfessionalType.Nurse => "Enfermagem",
        ProfessionalType.SocialWorker => "Social",
        ProfessionalType.Educator => "Pedagógica",
        ProfessionalType.Other => "Administrativa",
        _ => "Administrativa"
    };
}
