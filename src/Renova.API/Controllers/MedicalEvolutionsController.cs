using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
[Authorize(Policy = "ClinicalAccess")]
[Route("students/{studentId:guid}/medical-evolutions")]
public class MedicalEvolutionsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public MedicalEvolutionsController(AppDbContext dbContext)
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

        var medicalEvolutions = await _dbContext.MedicalEvolutions
            .Where(evolution => evolution.StudentId == studentId && !evolution.IsDeleted)
            .OrderByDescending(evolution => evolution.CreatedAt)
            .ToListAsync();

        return Ok(medicalEvolutions.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid studentId, Guid id)
    {
        var medicalEvolution = await _dbContext.MedicalEvolutions
            .FirstOrDefaultAsync(evolution =>
                evolution.StudentId == studentId &&
                evolution.Id == id &&
                !evolution.IsDeleted);

        if (medicalEvolution is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(medicalEvolution));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid studentId, CreateMedicalEvolutionRequest request)
    {
        if (!await StudentExists(studentId))
        {
            return NotFound();
        }

        var professionalExists = await _dbContext.Professionals
            .AnyAsync(professional => professional.Id == request.ProfessionalId && professional.IsActive);

        if (!professionalExists)
        {
            return BadRequest("Professional does not exist or is inactive.");
        }

        var medicalEvolution = new MedicalEvolution
        {
            StudentId = studentId,
            ProfessionalId = request.ProfessionalId,
            Description = request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.MedicalEvolutions.Add(medicalEvolution);
        await _dbContext.SaveChangesAsync();

        return Created(
            $"/students/{studentId}/medical-evolutions/{medicalEvolution.Id}",
            ToResponse(medicalEvolution));
    }

    private async Task<bool> StudentExists(Guid studentId)
    {
        return await _dbContext.Students.AnyAsync(student => student.Id == studentId);
    }

    private static MedicalEvolutionResponse ToResponse(MedicalEvolution medicalEvolution)
    {
        return new MedicalEvolutionResponse(
            medicalEvolution.Id,
            medicalEvolution.StudentId,
            medicalEvolution.ProfessionalId,
            medicalEvolution.Description,
            medicalEvolution.CreatedAt);
    }
}
