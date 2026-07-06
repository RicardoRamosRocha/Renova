using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
[Authorize(Policy = "ClinicalAccess")]
[Route("professionals")]
public class ProfessionalsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ProfessionalsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(bool includeInactive = false)
    {
        var query = _dbContext.Professionals
            .Include(professional => professional.Person)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(professional => professional.IsActive);
        }

        var professionals = await query
            .OrderBy(professional => professional.Person != null ? professional.Person.FullName : professional.FullName)
            .ToListAsync();

        return Ok(professionals.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var professional = await _dbContext.Professionals
            .Include(item => item.Person)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (professional is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(professional));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProfessionalRequest request)
    {
        var timestamp = DateTime.UtcNow;
        var professional = new Professional
        {
            FullName = request.FullName.Trim(),
            Specialty = request.Specialty.Trim(),
            RegistrationNumber = request.RegistrationNumber.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Phone = request.Phone.Trim(),
            IsActive = request.IsActive,
            CreatedAt = timestamp
        };

        professional.SyncPersonFromLegacyFields(timestamp);

        _dbContext.Professionals.Add(professional);
        await _dbContext.SaveChangesAsync();

        return Created($"/professionals/{professional.Id}", ToResponse(professional));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProfessionalRequest request)
    {
        var professional = await _dbContext.Professionals
            .Include(item => item.Person)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (professional is null)
        {
            return NotFound();
        }

        var timestamp = DateTime.UtcNow;
        professional.FullName = request.FullName.Trim();
        professional.Specialty = request.Specialty.Trim();
        professional.RegistrationNumber = request.RegistrationNumber.Trim();
        professional.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        professional.Phone = request.Phone.Trim();
        professional.IsActive = request.IsActive;
        professional.UpdatedAt = timestamp;
        professional.SyncPersonFromLegacyFields(timestamp, markPersonAsUpdated: true);

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(professional));
    }

    private static ProfessionalResponse ToResponse(Professional professional)
    {
        return new ProfessionalResponse(
            professional.Id,
            professional.DisplayName,
            professional.Specialty,
            professional.RegistrationNumber,
            professional.DisplayEmail,
            professional.DisplayPhone,
            professional.IsActive,
            professional.CreatedAt,
            professional.UpdatedAt);
    }
}
