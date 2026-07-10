using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Renova.Infrastructure.Data.Seed;

public static class TenantBootstrapSeedExtensions
{
    public static async Task SeedTenantBootstrapAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await TenantBootstrapSeeder.SeedAsync(scope.ServiceProvider);
    }

    public static async Task SeedTenantUserClaimsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await TenantUserClaimSeeder.SeedAsync(scope.ServiceProvider);
    }
}
