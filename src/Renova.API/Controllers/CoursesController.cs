using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
[Route("courses")]
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public CoursesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize(Policy = "CourseAccess")]
    public async Task<IActionResult> GetAll(bool includeInactive = false)
    {
        var query = _dbContext.Courses.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(course => course.IsActive);
        }

        var courses = await query
            .OrderBy(course => course.Title)
            .ToListAsync();

        return Ok(courses.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CourseAccess")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var course = await _dbContext.Courses.FindAsync(id);

        if (course is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(course));
    }

    [HttpPost]
    [Authorize(Policy = "CourseManagement")]
    public async Task<IActionResult> Create(CreateCourseRequest request)
    {
        var course = new Course
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Courses.Add(course);
        await _dbContext.SaveChangesAsync();

        return Created($"/courses/{course.Id}", ToResponse(course));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CourseManagement")]
    public async Task<IActionResult> Update(Guid id, UpdateCourseRequest request)
    {
        var course = await _dbContext.Courses.FindAsync(id);

        if (course is null)
        {
            return NotFound();
        }

        course.Title = request.Title.Trim();
        course.Description = request.Description.Trim();
        course.IsActive = request.IsActive;
        course.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(course));
    }

    private static CourseResponse ToResponse(Course course)
    {
        return new CourseResponse(
            course.Id,
            course.Title,
            course.Description,
            course.IsActive,
            course.CreatedAt,
            course.UpdatedAt);
    }
}
