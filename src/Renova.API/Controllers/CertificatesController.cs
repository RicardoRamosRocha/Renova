using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
public class CertificatesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public CertificatesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("students/{studentId:guid}/certificates")]
    [Authorize(Policy = "StudentLearning")]
    public async Task<IActionResult> GetByStudent(Guid studentId)
    {
        if (!await _dbContext.Students.AnyAsync(student => student.Id == studentId))
        {
            return NotFound();
        }

        var certificates = await _dbContext.Certificates
            .Where(certificate => certificate.StudentId == studentId)
            .OrderByDescending(certificate => certificate.IssuedAt)
            .ToListAsync();

        return Ok(certificates.Select(ToResponse));
    }

    [HttpGet("certificates/verify/{verificationCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> Verify(string verificationCode)
    {
        var certificate = await _dbContext.Certificates
            .FirstOrDefaultAsync(item => item.VerificationCode == verificationCode.Trim());

        if (certificate is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(certificate));
    }

    [HttpPost("students/{studentId:guid}/certificates")]
    [Authorize(Policy = "StudentLearning")]
    public async Task<IActionResult> Issue(Guid studentId, IssueCertificateRequest request)
    {
        if (!await _dbContext.Students.AnyAsync(student => student.Id == studentId))
        {
            return NotFound();
        }

        var course = await _dbContext.Courses
            .Include(item => item.Modules)
                .ThenInclude(module => module.Lessons)
            .FirstOrDefaultAsync(item => item.Id == request.CourseId);

        if (course is null)
        {
            return BadRequest("Course does not exist.");
        }

        var lessonIds = course.Modules
            .SelectMany(module => module.Lessons)
            .Select(lesson => lesson.Id)
            .ToArray();

        if (lessonIds.Length == 0)
        {
            return BadRequest("Course has no lessons.");
        }

        var completedLessonCount = await _dbContext.StudentProgress
            .CountAsync(progress =>
                progress.StudentId == studentId &&
                lessonIds.Contains(progress.LessonId) &&
                progress.CompletedAt != null);

        if (completedLessonCount < lessonIds.Length)
        {
            return BadRequest("Student has not completed all course lessons.");
        }

        var existingCertificate = await _dbContext.Certificates
            .FirstOrDefaultAsync(item => item.StudentId == studentId && item.CourseId == request.CourseId);

        if (existingCertificate is not null)
        {
            return Ok(ToResponse(existingCertificate));
        }

        var certificate = new Certificate
        {
            StudentId = studentId,
            CourseId = request.CourseId,
            VerificationCode = GenerateVerificationCode(),
            IssuedAt = DateTime.UtcNow
        };

        _dbContext.Certificates.Add(certificate);
        await _dbContext.SaveChangesAsync();

        return Created(
            $"/certificates/verify/{certificate.VerificationCode}",
            ToResponse(certificate));
    }

    private static string GenerateVerificationCode()
    {
        return $"RENOVA-{Guid.NewGuid():N}"[..39].ToUpperInvariant();
    }

    private static CertificateResponse ToResponse(Certificate certificate)
    {
        return new CertificateResponse(
            certificate.Id,
            certificate.StudentId,
            certificate.CourseId,
            certificate.VerificationCode,
            certificate.IssuedAt);
    }
}
