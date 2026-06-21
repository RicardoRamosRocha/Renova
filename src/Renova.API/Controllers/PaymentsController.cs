using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renova.API.DTOs;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.API.Controllers;

[ApiController]
[Authorize(Policy = "FinancialManagement")]
[Route("students/{studentId:guid}/payments")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public PaymentsController(AppDbContext dbContext)
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

        var payments = await _dbContext.Payments
            .Where(payment => payment.StudentId == studentId)
            .OrderByDescending(payment => payment.CreatedAt)
            .ToListAsync();

        return Ok(payments.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid studentId, Guid id)
    {
        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(item => item.StudentId == studentId && item.Id == id);

        if (payment is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(payment));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid studentId, CreatePaymentRequest request)
    {
        if (!await StudentExists(studentId))
        {
            return NotFound();
        }

        var payment = new Payment
        {
            StudentId = studentId,
            Amount = request.Amount,
            Status = request.Status,
            PaymentMethod = request.PaymentMethod,
            ExternalPaymentId = string.IsNullOrWhiteSpace(request.ExternalPaymentId)
                ? null
                : request.ExternalPaymentId.Trim(),
            PaidAt = request.PaidAt is null ? null : ToUtc(request.PaidAt.Value),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        return Created($"/students/{studentId}/payments/{payment.Id}", ToResponse(payment));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid studentId, Guid id, UpdatePaymentRequest request)
    {
        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(item => item.StudentId == studentId && item.Id == id);

        if (payment is null)
        {
            return NotFound();
        }

        payment.Amount = request.Amount;
        payment.Status = request.Status;
        payment.PaymentMethod = request.PaymentMethod;
        payment.ExternalPaymentId = string.IsNullOrWhiteSpace(request.ExternalPaymentId)
            ? null
            : request.ExternalPaymentId.Trim();
        payment.PaidAt = request.PaidAt is null ? null : ToUtc(request.PaidAt.Value);
        payment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(payment));
    }

    private async Task<bool> StudentExists(Guid studentId)
    {
        return await _dbContext.Students.AnyAsync(student => student.Id == studentId);
    }

    private static PaymentResponse ToResponse(Payment payment)
    {
        return new PaymentResponse(
            payment.Id,
            payment.StudentId,
            payment.Amount,
            payment.Status,
            payment.PaymentMethod,
            payment.ExternalPaymentId,
            payment.PaidAt,
            payment.CreatedAt,
            payment.UpdatedAt);
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
