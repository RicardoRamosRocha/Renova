using Renova.Infrastructure.Identity;

namespace Renova.Web.Services;

public class UserSession
{
    public ApplicationUser? User { get; private set; }

    public IReadOnlyCollection<string> Roles { get; private set; } = [];

    public bool IsAuthenticated => User is not null;

    public bool CanManageUsers => IsInRole(ApplicationRoles.Administrator);

    public bool CanManageStudents => IsInAnyRole(
        ApplicationRoles.Administrator,
        ApplicationRoles.Coordinator,
        ApplicationRoles.Attendant);

    public bool CanViewStudentRecords => IsInAnyRole(
        ApplicationRoles.Administrator,
        ApplicationRoles.Coordinator,
        ApplicationRoles.Professional,
        ApplicationRoles.Attendant);

    public bool CanManageProfessionals => IsInAnyRole(
        ApplicationRoles.Administrator,
        ApplicationRoles.Coordinator);

    public bool CanManageEad => IsInAnyRole(
        ApplicationRoles.Administrator,
        ApplicationRoles.Teacher);

    public bool CanManageFinance => IsInRole(ApplicationRoles.Administrator);

    public bool IsInRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsInAnyRole(params string[] roles)
    {
        return roles.Any(IsInRole);
    }

    public event Action? Changed;

    public void SignIn(ApplicationUser user, IReadOnlyCollection<string> roles)
    {
        User = user;
        Roles = roles;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        User = null;
        Roles = [];
        Changed?.Invoke();
    }
}
