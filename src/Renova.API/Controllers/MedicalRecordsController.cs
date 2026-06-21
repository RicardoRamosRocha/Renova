using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
[Route("students/{studentId:guid}/medical-record")]
public class MedicalRecordsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public MedicalRecordsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid studentId)
    {
        var medicalRecord = await _dbContext.MedicalRecords
            .FirstOrDefaultAsync(record => record.StudentId == studentId);

        if (medicalRecord is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(medicalRecord));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid studentId, CreateMedicalRecordRequest request)
    {
        if (!await StudentExists(studentId))
        {
            return NotFound();
        }

        var alreadyExists = await _dbContext.MedicalRecords
            .AnyAsync(record => record.StudentId == studentId);

        if (alreadyExists)
        {
            return Conflict("Student already has a medical record.");
        }

        var medicalRecord = new MedicalRecord
        {
            StudentId = studentId,
            Anamnesis = request.Anamnesis.Trim(),
            ClinicalNotes = request.ClinicalNotes.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.MedicalRecords.Add(medicalRecord);
        await _dbContext.SaveChangesAsync();

        return Created($"/students/{studentId}/medical-record", ToResponse(medicalRecord));
    }

    [HttpPut]
    public async Task<IActionResult> Update(Guid studentId, UpdateMedicalRecordRequest request)
    {
        var medicalRecord = await _dbContext.MedicalRecords
            .FirstOrDefaultAsync(record => record.StudentId == studentId);

        if (medicalRecord is null)
        {
            return NotFound();
        }

        medicalRecord.Anamnesis = request.Anamnesis.Trim();
        medicalRecord.ClinicalNotes = request.ClinicalNotes.Trim();
        medicalRecord.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(medicalRecord));
    }

    private async Task<bool> StudentExists(Guid studentId)
    {
        return await _dbContext.Students.AnyAsync(student => student.Id == studentId);
    }

    private static MedicalRecordResponse ToResponse(MedicalRecord medicalRecord)
    {
        return new MedicalRecordResponse(
            medicalRecord.Id,
            medicalRecord.StudentId,
            medicalRecord.Anamnesis,
            medicalRecord.ClinicalNotes,
            medicalRecord.CreatedAt,
            medicalRecord.UpdatedAt);
    }
}
