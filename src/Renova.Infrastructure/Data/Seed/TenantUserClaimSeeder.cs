using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Renova.Infrastructure.Identity;

namespace Renova.Infrastructure.Data.Seed;

public sealed class TenantUserClaimSeeder
{
    public const string TenantIdClaimType = "TenantId";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<TenantUserClaimSeeder>>();

        try
        {
            var db = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            var tenantId = await ResolveDefaultTenantIdAsync(db);
            if (!tenantId.HasValue)
            {
                logger.LogWarning("Could not resolve a single default tenant for administrator claims.");
                return;
            }

            var users = await userManager.Users
                .Where(user => user.IsActive)
                .ToListAsync();

            foreach (var user in users)
            {
                var claims = await userManager.GetClaimsAsync(user);
                var tenantClaims = claims
                    .Where(claim => claim.Type == TenantIdClaimType)
                    .ToList();

                if (await HasValidTenantClaimAsync(db, tenantClaims))
                {
                    continue;
                }

                foreach (var tenantClaim in tenantClaims)
                {
                    var removeResult = await userManager.RemoveClaimAsync(user, tenantClaim);
                    ThrowIfFailed(removeResult, $"Could not remove invalid tenant claim from user {user.Email}.");
                }

                var addResult = await userManager.AddClaimAsync(
                    user,
                    new Claim(TenantIdClaimType, tenantId.Value.ToString()));

                ThrowIfFailed(addResult, $"Could not add tenant claim to user {user.Email}.");
            }

            logger.LogInformation("Tenant user claim seed finished.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Tenant user claim seed failed.");
            throw;
        }
    }

    private static async Task<Guid?> ResolveDefaultTenantIdAsync(AppDbContext db)
    {
        var demoTenantId = await db.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.Name == TenantBootstrapSeeder.DefaultTenantName && tenant.IsActive)
            .Select(tenant => (Guid?)tenant.Id)
            .FirstOrDefaultAsync();

        if (demoTenantId.HasValue)
        {
            return demoTenantId;
        }

        var activeTenants = await db.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.IsActive)
            .OrderBy(tenant => tenant.CreatedAt)
            .Select(tenant => tenant.Id)
            .Take(2)
            .ToListAsync();

        return activeTenants.Count == 1 ? activeTenants[0] : null;
    }

    private static async Task<bool> HasValidTenantClaimAsync(AppDbContext db, IReadOnlyCollection<Claim> tenantClaims)
    {
        foreach (var tenantClaim in tenantClaims)
        {
            if (!Guid.TryParse(tenantClaim.Value, out var tenantId))
            {
                continue;
            }

            if (await db.Tenants.AnyAsync(tenant => tenant.Id == tenantId && tenant.IsActive))
            {
                return true;
            }
        }

        return false;
    }

    private static void ThrowIfFailed(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{message} {errors}");
    }
}
