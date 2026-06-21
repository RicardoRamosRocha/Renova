using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
[Route("students")]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public StudentsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStudentRequest request)
    {
        var errors = ValidateStudentRequest(request);
        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await _dbContext.Students
            .OrderBy(student => student.FullName)
            .ToListAsync();

        return Ok(students.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var student = await _dbContext.Students.FindAsync(id);

        if (student is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(student));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CreateStudentRequest request)
    {
        var student = await _dbContext.Students.FindAsync(id);

        if (student is null)
        {
            return NotFound();
        }

        var errors = ValidateStudentRequest(request);
        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
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
            student.FullName,
            student.BirthDate,
            student.CPF,
            student.Phone,
            student.Email,
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

    private static Dictionary<string, string[]> ValidateStudentRequest(CreateStudentRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        AddRequiredError(errors, request.FullName, nameof(request.FullName));
        AddRequiredError(errors, request.CPF, nameof(request.CPF));
        AddRequiredError(errors, request.Phone, nameof(request.Phone));

        AddMaxLengthError(errors, request.FullName, nameof(request.FullName), 200);
        AddMaxLengthError(errors, request.CPF, nameof(request.CPF), 14);
        AddMaxLengthError(errors, request.Phone, nameof(request.Phone), 20);
        AddMaxLengthError(errors, request.Email, nameof(request.Email), 200);
        AddMaxLengthError(errors, request.Address, nameof(request.Address), 500);

        if (request.BirthDate == default)
        {
            errors[nameof(request.BirthDate)] = ["A data de nascimento e obrigatoria."];
        }

        if (request.AdmissionDate == default)
        {
            errors[nameof(request.AdmissionDate)] = ["A data de admissao e obrigatoria."];
        }

        return errors;
    }

    private static void AddRequiredError(
        Dictionary<string, string[]> errors,
        string? value,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[fieldName] = ["Campo obrigatorio."];
        }
    }

    private static void AddMaxLengthError(
        Dictionary<string, string[]> errors,
        string? value,
        string fieldName,
        int maxLength)
    {
        if (value?.Trim().Length > maxLength)
        {
            errors[fieldName] = [$"Deve ter no maximo {maxLength} caracteres."];
        }
    }
}