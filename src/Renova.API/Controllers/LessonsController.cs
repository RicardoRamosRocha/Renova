using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
[Route("courses/{courseId:guid}/modules/{moduleId:guid}/lessons")]
public class LessonsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public LessonsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize(Policy = "CourseAccess")]
    public async Task<IActionResult> GetAll(Guid courseId, Guid moduleId)
    {
        if (!await ModuleBelongsToCourse(courseId, moduleId))
        {
            return NotFound();
        }

        var lessons = await _dbContext.Lessons
            .Where(lesson => lesson.CourseModuleId == moduleId)
            .OrderBy(lesson => lesson.Order)
            .ToListAsync();

        return Ok(lessons.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CourseAccess")]
    public async Task<IActionResult> GetById(Guid courseId, Guid moduleId, Guid id)
    {
        if (!await ModuleBelongsToCourse(courseId, moduleId))
        {
            return NotFound();
        }

        var lesson = await _dbContext.Lessons
            .FirstOrDefaultAsync(item => item.CourseModuleId == moduleId && item.Id == id);

        if (lesson is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(lesson));
    }

    [HttpPost]
    [Authorize(Policy = "CourseManagement")]
    public async Task<IActionResult> Create(Guid courseId, Guid moduleId, CreateLessonRequest request)
    {
        if (!await ModuleBelongsToCourse(courseId, moduleId))
        {
            return NotFound();
        }

        var lesson = new Lesson
        {
            CourseModuleId = moduleId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            VideoProvider = request.VideoProvider.Trim(),
            VideoExternalId = request.VideoExternalId.Trim(),
            DurationInMinutes = request.DurationInMinutes,
            Order = request.Order,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Lessons.Add(lesson);
        await _dbContext.SaveChangesAsync();

        return Created(
            $"/courses/{courseId}/modules/{moduleId}/lessons/{lesson.Id}",
            ToResponse(lesson));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CourseManagement")]
    public async Task<IActionResult> Update(Guid courseId, Guid moduleId, Guid id, UpdateLessonRequest request)
    {
        if (!await ModuleBelongsToCourse(courseId, moduleId))
        {
            return NotFound();
        }

        var lesson = await _dbContext.Lessons
            .FirstOrDefaultAsync(item => item.CourseModuleId == moduleId && item.Id == id);

        if (lesson is null)
        {
            return NotFound();
        }

        lesson.Title = request.Title.Trim();
        lesson.Description = request.Description.Trim();
        lesson.VideoProvider = request.VideoProvider.Trim();
        lesson.VideoExternalId = request.VideoExternalId.Trim();
        lesson.DurationInMinutes = request.DurationInMinutes;
        lesson.Order = request.Order;
        lesson.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(lesson));
    }

    private async Task<bool> ModuleBelongsToCourse(Guid courseId, Guid moduleId)
    {
        return await _dbContext.CourseModules
            .AnyAsync(module => module.Id == moduleId && module.CourseId == courseId);
    }

    private static LessonResponse ToResponse(Lesson lesson)
    {
        return new LessonResponse(
            lesson.Id,
            lesson.CourseModuleId,
            lesson.Title,
            lesson.Description,
            lesson.VideoProvider,
            lesson.VideoExternalId,
            lesson.DurationInMinutes,
            lesson.Order,
            lesson.CreatedAt,
            lesson.UpdatedAt);
    }
}
