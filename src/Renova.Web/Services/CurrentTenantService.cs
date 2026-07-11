using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Renova.Infrastructure.Data;
using Renova.Infrastructure.Identity;

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
    UserManager<ApplicationUser> userManager,
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
        var claimTenantId = await ResolveTenantFromClaimAsync(claimValue);
        if (claimTenantId.HasValue)
        {
            tenantId = claimTenantId;
            return tenantId;
        }

        var userTenantId = await ResolveTenantFromApplicationUserAsync();
        if (userTenantId.HasValue)
        {
            tenantId = userTenantId;
            return tenantId;
        }

        if (!environment.IsDevelopment())
        {
            return null;
        }

        tenantId = await ResolveSingleActiveTenantAsync();
        return tenantId;
    }

    private async Task<Guid?> ResolveTenantFromClaimAsync(string? claimValue)
    {
        if (!Guid.TryParse(claimValue, out var claimTenantId) || claimTenantId == Guid.Empty)
        {
            return null;
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        return await db.Tenants.AnyAsync(item => item.Id == claimTenantId && item.IsActive)
            ? claimTenantId
            : null;
    }

    private async Task<Guid?> ResolveTenantFromApplicationUserAsync()
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var tenantClaims = await userManager.GetClaimsAsync(user);
        foreach (var claim in tenantClaims.Where(item => item.Type == TenantIdClaimType))
        {
            var resolvedTenantId = await ResolveTenantFromClaimAsync(claim.Value);
            if (resolvedTenantId.HasValue)
            {
                return resolvedTenantId;
            }
        }

        return null;
    }

    private async Task<Guid?> ResolveSingleActiveTenantAsync()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var activeTenants = await db.Tenants
            .AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.CreatedAt)
            .Select(item => item.Id)
            .Take(2)
            .ToListAsync();

        return activeTenants.Count == 1 ? activeTenants[0] : null;
    }
}
