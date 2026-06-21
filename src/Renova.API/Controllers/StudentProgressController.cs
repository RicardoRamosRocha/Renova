using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
[Authorize(Policy = "StudentLearning")]
[Route("students/{studentId:guid}/progress")]
public class StudentProgressController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public StudentProgressController(AppDbContext dbContext)
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

        var progressEntries = await _dbContext.StudentProgress
            .Where(progress => progress.StudentId == studentId)
            .OrderByDescending(progress => progress.UpdatedAt ?? progress.CreatedAt)
            .ToListAsync();

        return Ok(progressEntries.Select(ToResponse));
    }

    [HttpGet("lessons/{lessonId:guid}")]
    public async Task<IActionResult> GetByLesson(Guid studentId, Guid lessonId)
    {
        var progress = await _dbContext.StudentProgress
            .FirstOrDefaultAsync(item => item.StudentId == studentId && item.LessonId == lessonId);

        if (progress is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(progress));
    }

    [HttpPut]
    public async Task<IActionResult> Upsert(Guid studentId, UpdateStudentProgressRequest request)
    {
        if (!await StudentExists(studentId))
        {
            return NotFound();
        }

        if (!await LessonExists(request.LessonId))
        {
            return BadRequest("Lesson does not exist.");
        }

        var progress = await _dbContext.StudentProgress
            .FirstOrDefaultAsync(item => item.StudentId == studentId && item.LessonId == request.LessonId);

        if (progress is null)
        {
            progress = new StudentProgress
            {
                StudentId = studentId,
                LessonId = request.LessonId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.StudentProgress.Add(progress);
        }
        else
        {
            progress.UpdatedAt = DateTime.UtcNow;
        }

        progress.WatchedPercentage = request.WatchedPercentage;

        if (request.WatchedPercentage >= 100 && progress.CompletedAt is null)
        {
            progress.CompletedAt = DateTime.UtcNow;
        }
        else if (request.WatchedPercentage < 100)
        {
            progress.CompletedAt = null;
        }

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(progress));
    }

    private async Task<bool> StudentExists(Guid studentId)
    {
        return await _dbContext.Students.AnyAsync(student => student.Id == studentId);
    }

    private async Task<bool> LessonExists(Guid lessonId)
    {
        return await _dbContext.Lessons.AnyAsync(lesson => lesson.Id == lessonId);
    }

    private static StudentProgressResponse ToResponse(StudentProgress progress)
    {
        return new StudentProgressResponse(
            progress.Id,
            progress.StudentId,
            progress.LessonId,
            progress.WatchedPercentage,
            progress.CompletedAt,
            progress.CreatedAt,
            progress.UpdatedAt);
    }
}
