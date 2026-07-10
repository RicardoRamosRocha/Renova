using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Renova.Infrastructure.Data;

namespace Renova.Web.Services;

public interface ICurrentTenantService
{
    bool HasTenant { get; }

    Guid? TenantId { get; }

    Task<Guid?> GetTenantIdAsync();
}

public sealed class CurrentTenantService(
    IHttpContextAccessor httpContextAccessor,
    IDbContextFactory<AppDbContext> dbContextFactory,
    IWebHostEnvironment environment) : ICurrentTenantService
{
    public const string TenantIdClaimType = "TenantId";

    private bool resolved;
    private Guid? tenantId;

    public bool HasTenant => TenantId.HasValue;

    public Guid? TenantId => resolved ? tenantId : null;

    public async Task<Guid?> GetTenantIdAsync()
    {
        if (resolved)
        {
            return tenantId;
        }

        resolved = true;

        var claimValue = httpContextAccessor.HttpContext?.User.FindFirstValue(TenantIdClaimType);
        if (Guid.TryParse(claimValue, out var claimTenantId))
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            if (await db.Tenants.AnyAsync(item => item.Id == claimTenantId && item.IsActive))
            {
                tenantId = claimTenantId;
                return tenantId;
            }
        }

        if (!environment.IsDevelopment())
        {
            return null;
        }

        await using (var fallbackDb = await dbContextFactory.CreateDbContextAsync())
        {
            var activeTenants = await fallbackDb.Tenants
                .AsNoTracking()
                .Where(item => item.IsActive)
                .OrderBy(item => item.CreatedAt)
                .Select(item => item.Id)
                .Take(2)
                .ToListAsync();

            tenantId = activeTenants.Count == 1 ? activeTenants[0] : null;
        }

        return tenantId;
    }
}
