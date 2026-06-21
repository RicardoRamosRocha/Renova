using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
[Authorize(Policy = "ClinicalAccess")]
[Route("students/{studentId:guid}/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AppointmentsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid studentId)
    {
        if (!await StudentExists(studentId))
        {
            return NotFound();
        }

        var appointments = await _dbContext.Appointments
            .Where(appointment => appointment.StudentId == studentId)
            .OrderByDescending(appointment => appointment.ScheduledAt)
            .ToListAsync();

        return Ok(appointments.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid studentId, Guid id)
    {
        var appointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(item => item.StudentId == studentId && item.Id == id);

        if (appointment is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(appointment));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid studentId, CreateAppointmentRequest request)
    {
        if (!await StudentExists(studentId))
        {
            return NotFound();
        }

        if (!await ProfessionalCanBeUsed(request.ProfessionalId))
        {
            return BadRequest("Professional does not exist or is inactive.");
        }

        var appointment = new Appointment
        {
            StudentId = studentId,
            ProfessionalId = request.ProfessionalId,
            ScheduledAt = ToUtc(request.ScheduledAt),
            Status = request.Status,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Appointments.Add(appointment);
        await _dbContext.SaveChangesAsync();

        return Created($"/students/{studentId}/appointments/{appointment.Id}", ToResponse(appointment));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid studentId, Guid id, UpdateAppointmentRequest request)
    {
        var appointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(item => item.StudentId == studentId && item.Id == id);

        if (appointment is null)
        {
            return NotFound();
        }

        if (!await ProfessionalCanBeUsed(request.ProfessionalId))
        {
            return BadRequest("Professional does not exist or is inactive.");
        }

        appointment.ProfessionalId = request.ProfessionalId;
        appointment.ScheduledAt = ToUtc(request.ScheduledAt);
        appointment.Status = request.Status;
        appointment.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        appointment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(appointment));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid studentId, Guid id)
    {
        var appointment = await _dbContext.Appointments
            .FirstOrDefaultAsync(item => item.StudentId == studentId && item.Id == id);

        if (appointment is null)
        {
            return NotFound();
        }

        _dbContext.Appointments.Remove(appointment);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> StudentExists(Guid studentId)
    {
        return await _dbContext.Students.AnyAsync(student => student.Id == studentId);
    }

    private async Task<bool> ProfessionalCanBeUsed(Guid? professionalId)
    {
        if (professionalId is null)
        {
            return true;
        }

        return await _dbContext.Professionals
            .AnyAsync(professional => professional.Id == professionalId && professional.IsActive);
    }

    private static AppointmentResponse ToResponse(Appointment appointment)
    {
        return new AppointmentResponse(
            appointment.Id,
            appointment.StudentId,
            appointment.ProfessionalId,
            appointment.ScheduledAt,
            appointment.Status,
            appointment.Notes,
            appointment.CreatedAt,
            appointment.UpdatedAt);
    }

    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
