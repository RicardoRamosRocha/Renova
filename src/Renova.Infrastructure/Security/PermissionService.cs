using Microsoft.EntityFrameworkCore;
using Renova.Application.Security;
using Renova.Domain.Entities;
using Renova.Infrastructure.Data;

namespace Renova.Infrastructure.Security;

public sealed class PermissionService(AppDbContext db) : IPermissionService
{
    public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionKey)
    {
        if (string.IsNullOrWhiteSpace(permissionKey))
        {
            return false;
        }

        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permissionKey, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId)
    {
        var userIdValue = userId.ToString();

        var roleIds = await db.UserRoles
            .Where(userRole => userRole.UserId == userIdValue)
            .Select(userRole => userRole.RoleId)
            .ToListAsync();

        var rolePermissions = await db.RolePermissions
            .Where(rolePermission => roleIds.Contains(rolePermission.RoleId))
            .Select(rolePermission => rolePermission.Permission.Key)
            .ToListAsync();

        var userPermissions = await db.UserPermissions
            .Where(userPermission => userPermission.UserId == userIdValue)
            .Select(userPermission => userPermission.Permission.Key)
            .ToListAsync();

        return rolePermissions
            .Concat(userPermissions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(permission => permission)
            .ToList();
    }

    public async Task<IReadOnlyList<Module>> GetAllowedModulesAsync(Guid userId)
    {
        var permissionKeys = await GetUserPermissionsAsync(userId);

        if (permissionKeys.Count == 0)
        {
            return [];
        }

        return await db.Modules
            .Where(module => module.IsActive &&
                module.Permissions.Any(permission => permissionKeys.Contains(permission.Key)))
            .OrderBy(module => module.DisplayOrder)
            .ThenBy(module => module.Name)
            .ToListAsync();
    }
}
