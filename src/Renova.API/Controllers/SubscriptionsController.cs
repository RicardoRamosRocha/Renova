using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
[Authorize(Policy = "FinancialManagement")]
[Route("students/{studentId:guid}/subscriptions")]
public class SubscriptionsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public SubscriptionsController(AppDbContext dbContext)
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

        var subscriptions = await _dbContext.Subscriptions
            .Where(subscription => subscription.StudentId == studentId)
            .OrderBy(subscription => subscription.NextDueDate)
            .ToListAsync();

        return Ok(subscriptions.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid studentId, Guid id)
    {
        var subscription = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(item => item.StudentId == studentId && item.Id == id);

        if (subscription is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(subscription));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid studentId, CreateSubscriptionRequest request)
    {
        if (!await StudentExists(studentId))
        {
            return NotFound();
        }

        var subscription = new Subscription
        {
            StudentId = studentId,
            PlanName = request.PlanName.Trim(),
            Amount = request.Amount,
            Status = request.Status,
            NextDueDate = ToUtc(request.NextDueDate),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Subscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync();

        return Created($"/students/{studentId}/subscriptions/{subscription.Id}", ToResponse(subscription));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid studentId, Guid id, UpdateSubscriptionRequest request)
    {
        var subscription = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(item => item.StudentId == studentId && item.Id == id);

        if (subscription is null)
        {
            return NotFound();
        }

        subscription.PlanName = request.PlanName.Trim();
        subscription.Amount = request.Amount;
        subscription.Status = request.Status;
        subscription.NextDueDate = ToUtc(request.NextDueDate);
        subscription.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(subscription));
    }

    private async Task<bool> StudentExists(Guid studentId)
    {
        return await _dbContext.Students.AnyAsync(student => student.Id == studentId);
    }

    private static SubscriptionResponse ToResponse(Subscription subscription)
    {
        return new SubscriptionResponse(
            subscription.Id,
            subscription.StudentId,
            subscription.PlanName,
            subscription.Amount,
            subscription.Status,
            subscription.NextDueDate,
            subscription.CreatedAt,
            subscription.UpdatedAt);
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
