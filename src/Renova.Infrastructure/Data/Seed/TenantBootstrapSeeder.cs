using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Seed;

public sealed class TenantBootstrapSeeder
{
    public const string DefaultTenantName = "Comunidade Renova Demo";
    public const string DefaultTenantCnpj = "00.000.000/0001-00";
    public const string DefaultTenantEmail = "demo@renova.com.br";
    public const string DefaultPlanName = "Plano Demonstração";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<TenantBootstrapSeeder>>();

        try
        {
            var db = services.GetRequiredService<AppDbContext>();
            var timestamp = DateTime.UtcNow;

            var tenant = await db.Tenants
                .IgnoreQueryFilters()
                .OrderBy(item => item.CreatedAt)
                .FirstOrDefaultAsync(item => !item.IsDeleted);

            if (tenant is null)
            {
                tenant = new Tenant
                {
                    Name = DefaultTenantName,
                    LegalName = DefaultTenantName,
                    Cnpj = DefaultTenantCnpj,
                    Email = DefaultTenantEmail,
                    Phone = "(31) 99999-9999",
                    City = "Belo Horizonte",
                    State = "MG",
                    IsActive = true,
                    CreatedAt = timestamp
                };

                db.Tenants.Add(tenant);
                logger.LogInformation("Creating default tenant {TenantName}.", DefaultTenantName);
            }
            else if (!tenant.IsActive)
            {
                tenant.IsActive = true;
                tenant.UpdatedAt = timestamp;
                logger.LogInformation("Reactivating tenant {TenantName}.", tenant.Name);
            }

            await db.SaveChangesAsync();

            await EnsureTenantSettingsAsync(db, tenant, timestamp);
            var plan = await EnsureSubscriptionPlanAsync(db, timestamp);
            await EnsureTenantSubscriptionAsync(db, tenant, plan, timestamp);

            await db.SaveChangesAsync();
            logger.LogInformation("Tenant bootstrap seed finished.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Tenant bootstrap seed failed.");
            throw;
        }
    }

    private static async Task EnsureTenantSettingsAsync(AppDbContext db, Tenant tenant, DateTime timestamp)
    {
        var settings = await db.TenantSettings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(item => item.TenantId == tenant.Id);

        if (settings is null)
        {
            db.TenantSettings.Add(new TenantSettings
            {
                TenantId = tenant.Id,
                PrimaryColor = "#2563EB",
                SecondaryColor = "#10B981",
                AllowFamilyPortal = true,
                AllowEad = true,
                AllowInventory = true,
                AllowTherapeuticPlans = true,
                CreatedAt = timestamp
            });

            return;
        }

        if (settings.IsDeleted)
        {
            settings.IsDeleted = false;
            settings.UpdatedAt = timestamp;
        }
    }

    private static async Task<SubscriptionPlan> EnsureSubscriptionPlanAsync(AppDbContext db, DateTime timestamp)
    {
        var plan = await db.SubscriptionPlans
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(item => item.Name == DefaultPlanName);

        if (plan is null)
        {
            plan = new SubscriptionPlan
            {
                Name = DefaultPlanName,
                Description = "Plano padrão para ambiente inicial do Renova.",
                MaxUsers = 50,
                MaxStudents = 500,
                HasEad = true,
                HasFinance = true,
                HasReports = true,
                HasFamilyPortal = true,
                HasInventory = true,
                HasTherapeuticPlans = true,
                MonthlyPrice = 0m,
                IsActive = true,
                CreatedAt = timestamp
            };

            db.SubscriptionPlans.Add(plan);
            await db.SaveChangesAsync();
            return plan;
        }

        var changed = false;

        if (!plan.IsActive)
        {
            plan.IsActive = true;
            changed = true;
        }

        if (plan.IsDeleted)
        {
            plan.IsDeleted = false;
            changed = true;
        }

        if (changed)
        {
            plan.UpdatedAt = timestamp;
        }

        return plan;
    }

    private static async Task EnsureTenantSubscriptionAsync(
        AppDbContext db,
        Tenant tenant,
        SubscriptionPlan plan,
        DateTime timestamp)
    {
        var subscription = await db.TenantSubscriptions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(item => item.TenantId == tenant.Id && item.SubscriptionPlanId == plan.Id);

        if (subscription is null)
        {
            db.TenantSubscriptions.Add(new TenantSubscription
            {
                TenantId = tenant.Id,
                SubscriptionPlanId = plan.Id,
                StartDate = timestamp,
                IsActive = true,
                Status = SubscriptionStatus.Trial,
                CreatedAt = timestamp
            });

            return;
        }

        var changed = false;

        if (!subscription.IsActive)
        {
            subscription.IsActive = true;
            changed = true;
        }

        if (subscription.IsDeleted)
        {
            subscription.IsDeleted = false;
            changed = true;
        }

        if (subscription.Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Expired)
        {
            subscription.Status = SubscriptionStatus.Trial;
            changed = true;
        }

        if (changed)
        {
            subscription.UpdatedAt = timestamp;
        }
    }
}
