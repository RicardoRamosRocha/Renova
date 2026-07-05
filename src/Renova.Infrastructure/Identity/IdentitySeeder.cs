using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Renova.Infrastructure.Identity;

public sealed class IdentitySeeder
{
    private const string AdminFullName = "Administrador Renova";
    private const string AdminEmail = "admin@renova.com.br";
    private const string AdminPassword = "Renova@2026";
    private const string AdminPhone = "(31) 99999-9999";

    private static readonly string[] RequiredRoles =
    [
        ApplicationRoles.Administrator,
        ApplicationRoles.Coordinator,
        ApplicationRoles.Professional,
        ApplicationRoles.Attendant,
        ApplicationRoles.Teacher,
        ApplicationRoles.Student,
        ApplicationRoles.FamilyMember
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<IdentitySeeder>>();

        try
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            await EnsureRolesAsync(roleManager, logger);
            await EnsureAdministratorAsync(userManager, logger);

            logger.LogInformation("Identity seed finished.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Identity seed failed.");
            throw;
        }
    }

    private static async Task EnsureRolesAsync(
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        foreach (var roleName in RequiredRoles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                logger.LogInformation("Role {RoleName} already exists.", roleName);
                continue;
            }

            logger.LogInformation("Creating role {RoleName}...", roleName);
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            ThrowIfFailed(result, $"Could not create role {roleName}.");
        }
    }

    private static async Task EnsureAdministratorAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        var administrator = await userManager.FindByEmailAsync(AdminEmail);

        if (administrator is null)
        {
            logger.LogInformation("Creating default administrator...");
            administrator = CreateAdministrator();

            var createResult = await userManager.CreateAsync(administrator, AdminPassword);
            ThrowIfFailed(createResult, "Could not create default administrator.");
        }
        else
        {
            logger.LogInformation("Administrator already exists.");
        }

        var administratorChanged = false;

        if (administrator.UserName != AdminEmail)
        {
            administrator.UserName = AdminEmail;
            administratorChanged = true;
        }

        if (administrator.Email != AdminEmail)
        {
            administrator.Email = AdminEmail;
            administratorChanged = true;
        }

        if (administrator.FullName != AdminFullName)
        {
            administrator.FullName = AdminFullName;
            administratorChanged = true;
        }

        if (administrator.PhoneNumber != AdminPhone)
        {
            administrator.PhoneNumber = AdminPhone;
            administratorChanged = true;
        }

        if (!administrator.IsActive)
        {
            administrator.IsActive = true;
            administratorChanged = true;
        }

        if (!administrator.EmailConfirmed)
        {
            administrator.EmailConfirmed = true;
            administratorChanged = true;
        }

        if (administratorChanged)
        {
            administrator.UpdatedAt = DateTime.UtcNow;
            var updateResult = await userManager.UpdateAsync(administrator);
            ThrowIfFailed(updateResult, "Could not update default administrator.");
        }

        if (!await userManager.CheckPasswordAsync(administrator, AdminPassword))
        {
            if (await userManager.HasPasswordAsync(administrator))
            {
                var removePasswordResult = await userManager.RemovePasswordAsync(administrator);
                ThrowIfFailed(removePasswordResult, "Could not reset default administrator password.");
            }

            var addPasswordResult = await userManager.AddPasswordAsync(administrator, AdminPassword);
            ThrowIfFailed(addPasswordResult, "Could not set default administrator password.");
        }

        if (!await userManager.IsInRoleAsync(administrator, ApplicationRoles.Administrator))
        {
            var roleResult = await userManager.AddToRoleAsync(administrator, ApplicationRoles.Administrator);
            ThrowIfFailed(roleResult, "Could not add default administrator to Administrador role.");
        }
    }

    private static ApplicationUser CreateAdministrator()
    {
        return new ApplicationUser
        {
            FullName = AdminFullName,
            UserName = AdminEmail,
            Email = AdminEmail,
            PhoneNumber = AdminPhone,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
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
