using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
[Route("courses/{courseId:guid}/modules")]
public class CourseModulesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public CourseModulesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize(Policy = "CourseAccess")]
    public async Task<IActionResult> GetAll(Guid courseId)
    {
        if (!await CourseExists(courseId))
        {
            return NotFound();
        }

        var modules = await _dbContext.CourseModules
            .Where(module => module.CourseId == courseId)
            .OrderBy(module => module.Order)
            .ToListAsync();

        return Ok(modules.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CourseAccess")]
    public async Task<IActionResult> GetById(Guid courseId, Guid id)
    {
        var module = await _dbContext.CourseModules
            .FirstOrDefaultAsync(item => item.CourseId == courseId && item.Id == id);

        if (module is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(module));
    }

    [HttpPost]
    [Authorize(Policy = "CourseManagement")]
    public async Task<IActionResult> Create(Guid courseId, CreateCourseModuleRequest request)
    {
        if (!await CourseExists(courseId))
        {
            return NotFound();
        }

        var module = new CourseModule
        {
            CourseId = courseId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Order = request.Order,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.CourseModules.Add(module);
        await _dbContext.SaveChangesAsync();

        return Created($"/courses/{courseId}/modules/{module.Id}", ToResponse(module));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CourseManagement")]
    public async Task<IActionResult> Update(Guid courseId, Guid id, UpdateCourseModuleRequest request)
    {
        var module = await _dbContext.CourseModules
            .FirstOrDefaultAsync(item => item.CourseId == courseId && item.Id == id);

        if (module is null)
        {
            return NotFound();
        }

        module.Title = request.Title.Trim();
        module.Description = request.Description.Trim();
        module.Order = request.Order;
        module.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(module));
    }

    private async Task<bool> CourseExists(Guid courseId)
    {
        return await _dbContext.Courses.AnyAsync(course => course.Id == courseId);
    }

    private static CourseModuleResponse ToResponse(CourseModule module)
    {
        return new CourseModuleResponse(
            module.Id,
            module.CourseId,
            module.Title,
            module.Description,
            module.Order,
            module.CreatedAt,
            module.UpdatedAt);
    }
}
