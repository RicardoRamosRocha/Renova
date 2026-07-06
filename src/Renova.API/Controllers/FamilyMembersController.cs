using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
[Authorize(Policy = "ClinicalAccess")]
[Route("students/{studentId:guid}/family-members")]
public class FamilyMembersController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public FamilyMembersController(AppDbContext dbContext)
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

        var familyMembers = await _dbContext.FamilyMembers
            .Include(familyMember => familyMember.Person)
            .Where(familyMember => familyMember.StudentId == studentId)
            .OrderBy(familyMember => familyMember.Person != null ? familyMember.Person.FullName : familyMember.FullName)
            .ToListAsync();

        return Ok(familyMembers.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid studentId, Guid id)
    {
        var familyMember = await _dbContext.FamilyMembers
            .Include(member => member.Person)
            .FirstOrDefaultAsync(member => member.StudentId == studentId && member.Id == id);

        if (familyMember is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(familyMember));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid studentId, CreateFamilyMemberRequest request)
    {
        var student = await _dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == studentId);

        if (student is null)
        {
            return NotFound();
        }

        var timestamp = DateTime.UtcNow;
        var familyMember = new FamilyMember
        {
            StudentId = studentId,
            TenantId = student.TenantId,
            FullName = request.FullName.Trim(),
            Relationship = request.Relationship.Trim(),
            Phone = request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            CanAccessPortal = request.CanAccessPortal,
            CreatedAt = timestamp
        };

        familyMember.SyncPersonFromLegacyFields(timestamp);

        _dbContext.FamilyMembers.Add(familyMember);
        await _dbContext.SaveChangesAsync();

        return Created(
            $"/students/{studentId}/family-members/{familyMember.Id}",
            ToResponse(familyMember));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid studentId, Guid id, UpdateFamilyMemberRequest request)
    {
        var familyMember = await _dbContext.FamilyMembers
            .Include(member => member.Person)
            .FirstOrDefaultAsync(member => member.StudentId == studentId && member.Id == id);

        if (familyMember is null)
        {
            return NotFound();
        }

        var timestamp = DateTime.UtcNow;
        familyMember.FullName = request.FullName.Trim();
        familyMember.Relationship = request.Relationship.Trim();
        familyMember.Phone = request.Phone.Trim();
        familyMember.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        familyMember.CanAccessPortal = request.CanAccessPortal;
        familyMember.UpdatedAt = timestamp;
        familyMember.SyncPersonFromLegacyFields(timestamp, markPersonAsUpdated: true);

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(familyMember));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid studentId, Guid id)
    {
        var familyMember = await _dbContext.FamilyMembers
            .FirstOrDefaultAsync(member => member.StudentId == studentId && member.Id == id);

        if (familyMember is null)
        {
            return NotFound();
        }

        _dbContext.FamilyMembers.Remove(familyMember);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> StudentExists(Guid studentId)
    {
        return await _dbContext.Students.AnyAsync(student => student.Id == studentId);
    }

    private static FamilyMemberResponse ToResponse(FamilyMember familyMember)
    {
        return new FamilyMemberResponse(
            familyMember.Id,
            familyMember.StudentId,
            familyMember.DisplayName,
            familyMember.Relationship,
            familyMember.DisplayPhone,
            familyMember.DisplayEmail,
            familyMember.CanAccessPortal,
            familyMember.CreatedAt,
            familyMember.UpdatedAt);
    }
}
