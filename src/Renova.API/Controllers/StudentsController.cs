using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
[Authorize(Policy = "ClinicalAccess")]
[Route("students")]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public StudentsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 20;
        }

        if (pageSize > 100)
        {
            pageSize = 100;
        }

        var students = await _dbContext.Students
            .Include(student => student.Person)
            .OrderBy(student => student.Person != null ? student.Person.FullName : student.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(students.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var student = await _dbContext.Students
            .Include(item => item.Person)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (student is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(student));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStudentRequest request)
    {
        var student = new Student
        {
            FullName = request.FullName.Trim(),
            BirthDate = ToUtc(request.BirthDate),
            CPF = request.CPF.Trim(),
            Phone = request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
            Status = request.Status,
            AdmissionDate = ToUtc(request.AdmissionDate),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync();

        return Created($"/students/{student.Id}", ToResponse(student));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateStudentRequest request)
    {
        var student = await _dbContext.Students.FindAsync(id);

        if (student is null)
        {
            return NotFound();
        }

        student.FullName = request.FullName.Trim();
        student.BirthDate = ToUtc(request.BirthDate);
        student.CPF = request.CPF.Trim();
        student.Phone = request.Phone.Trim();
        student.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        student.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        student.Status = request.Status;
        student.AdmissionDate = ToUtc(request.AdmissionDate);
        student.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(student));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var student = await _dbContext.Students.FindAsync(id);

        if (student is null)
        {
            return NotFound();
        }

        _dbContext.Students.Remove(student);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private static StudentResponse ToResponse(Student student)
    {
        return new StudentResponse(
            student.Id,
            student.DisplayName,
            student.DisplayBirthDate,
            student.DisplayCpf,
            student.DisplayPhone,
            student.DisplayEmail,
            student.Address,
            student.Status,
            student.AdmissionDate,
            student.CreatedAt,
            student.UpdatedAt);
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
