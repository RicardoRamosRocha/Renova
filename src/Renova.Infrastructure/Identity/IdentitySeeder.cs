using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Renova.Domain.Entities;
using Renova.Domain.Security;
using Renova.Infrastructure.Data;

namespace Renova.Infrastructure.Identity;

public sealed class IdentitySeeder
{
    private const string AdminFullName = "Administrador Renova";
    private const string AdminEmail = "admin@renova.com.br";
    private const string AdminPassword = "Renova@2026";
    private const string AdminPhone = "(31) 99999-9999";

    private static readonly string[] RequiredRoles =
    [
        ApplicationRoles.SuperAdmin,
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
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
            var db = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            await EnsureRolesAsync(roleManager, logger);
            await EnsureModulesAndPermissionsAsync(db, roleManager, logger);
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
        RoleManager<ApplicationRole> roleManager,
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
            var role = ApplicationRoles.Seed.FirstOrDefault(item => item.Name == roleName)
                ?? new ApplicationRole { Name = roleName, Description = roleName, IsSystemRole = true };
            var result = await roleManager.CreateAsync(role);
            ThrowIfFailed(result, $"Could not create role {roleName}.");
        }
    }

    private static async Task EnsureModulesAndPermissionsAsync(
        AppDbContext db,
        RoleManager<ApplicationRole> roleManager,
        ILogger logger)
    {
        var modules = SecuritySeedData.Modules;

        foreach (var moduleSeed in modules)
        {
            var module = await db.Modules.IgnoreQueryFilters().FirstOrDefaultAsync(item => item.Key == moduleSeed.Key);

            if (module is null)
            {
                db.Modules.Add(new Module
                {
                    Id = moduleSeed.Id,
                    Name = moduleSeed.Name,
                    Key = moduleSeed.Key,
                    Description = moduleSeed.Description,
                    Icon = moduleSeed.Icon,
                    DisplayOrder = moduleSeed.DisplayOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
                continue;
            }

            module.Name = moduleSeed.Name;
            module.Description = moduleSeed.Description;
            module.Icon = moduleSeed.Icon;
            module.DisplayOrder = moduleSeed.DisplayOrder;
            module.IsActive = true;
            module.IsDeleted = false;
            module.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        foreach (var permissionSeed in SecuritySeedData.Permissions)
        {
            var permission = await db.Permissions.IgnoreQueryFilters().FirstOrDefaultAsync(item => item.Key == permissionSeed.Key);

            if (permission is null)
            {
                db.Permissions.Add(new Permission
                {
                    Id = permissionSeed.Id,
                    Name = permissionSeed.Name,
                    Key = permissionSeed.Key,
                    Description = permissionSeed.Description,
                    ModuleId = permissionSeed.ModuleId,
                    CreatedAt = DateTime.UtcNow
                });
                continue;
            }

            permission.Name = permissionSeed.Name;
            permission.Description = permissionSeed.Description;
            permission.ModuleId = permissionSeed.ModuleId;
            permission.IsDeleted = false;
            permission.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        foreach (var menuPermissionSeed in SecuritySeedData.Permissions)
        {
            var exists = await db.MenuPermissions.IgnoreQueryFilters().AnyAsync(item =>
                item.ModuleId == menuPermissionSeed.ModuleId &&
                item.PermissionId == menuPermissionSeed.Id &&
                !item.IsDeleted);

            if (!exists)
            {
                db.MenuPermissions.Add(new MenuPermission
                {
                    ModuleId = menuPermissionSeed.ModuleId,
                    PermissionId = menuPermissionSeed.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();

        foreach (var roleName in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    Description = roleName,
                    IsSystemRole = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var permissionsByKey = await db.Permissions.IgnoreQueryFilters()
            .Where(permission => !permission.IsDeleted)
            .ToDictionaryAsync(permission => permission.Key, permission => permission.Id);

        foreach (var rolePermissions in SecuritySeedData.RolePermissions)
        {
            var role = await roleManager.FindByNameAsync(rolePermissions.Key);
            if (role is null)
            {
                logger.LogWarning("Role {RoleName} was not found while seeding permissions.", rolePermissions.Key);
                continue;
            }

            foreach (var permissionKey in rolePermissions.Value)
            {
                if (!permissionsByKey.TryGetValue(permissionKey, out var permissionId))
                {
                    logger.LogWarning("Permission {PermissionKey} was not found while seeding role permissions.", permissionKey);
                    continue;
                }

                var existing = await db.RolePermissions.IgnoreQueryFilters().FirstOrDefaultAsync(item =>
                    item.RoleId == role.Id &&
                    item.PermissionId == permissionId);

                if (existing is null)
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await db.SaveChangesAsync();
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

        if (!await userManager.IsInRoleAsync(administrator, ApplicationRoles.SuperAdmin))
        {
            var roleResult = await userManager.AddToRoleAsync(administrator, ApplicationRoles.SuperAdmin);
            ThrowIfFailed(roleResult, "Could not add default administrator to SuperAdmin role.");
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
