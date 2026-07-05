using Renova.Domain.Entities;

namespace Renova.Application.Security;

public interface IPermissionService
{
    Task<bool> UserHasPermissionAsync(Guid userId, string permissionKey);

    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId);

    Task<IReadOnlyList<Module>> GetAllowedModulesAsync(Guid userId);
}
