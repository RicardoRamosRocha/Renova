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
        var query = _dbContext.Professionals.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(professional => professional.IsActive);
        }

        var professionals = await query
            .OrderBy(professional => professional.FullName)
            .ToListAsync();

        return Ok(professionals.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var professional = await _dbContext.Professionals.FindAsync(id);

        if (professional is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(professional));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProfessionalRequest request)
    {
        var professional = new Professional
        {
            FullName = request.FullName.Trim(),
            Specialty = request.Specialty.Trim(),
            RegistrationNumber = request.RegistrationNumber.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Phone = request.Phone.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Professionals.Add(professional);
        await _dbContext.SaveChangesAsync();

        return Created($"/professionals/{professional.Id}", ToResponse(professional));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProfessionalRequest request)
    {
        var professional = await _dbContext.Professionals.FindAsync(id);

        if (professional is null)
        {
            return NotFound();
        }

        professional.FullName = request.FullName.Trim();
        professional.Specialty = request.Specialty.Trim();
        professional.RegistrationNumber = request.RegistrationNumber.Trim();
        professional.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        professional.Phone = request.Phone.Trim();
        professional.IsActive = request.IsActive;
        professional.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(professional));
    }

    private static ProfessionalResponse ToResponse(Professional professional)
    {
        return new ProfessionalResponse(
            professional.Id,
            professional.FullName,
            professional.Specialty,
            professional.RegistrationNumber,
            professional.Email,
            professional.Phone,
            professional.IsActive,
            professional.CreatedAt,
            professional.UpdatedAt);
    }
}
