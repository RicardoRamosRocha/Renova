using Renova.Infrastructure.Identity;

namespace Renova.Web.Services;

public class UserSession
{
    public ApplicationUser? User { get; private set; }

    public IReadOnlyCollection<string> Roles { get; private set; } = [];

    public bool IsAuthenticated => User is not null;

    public bool CanManageUsers => IsInAnyRole(ApplicationRoles.UserManagement);

    public bool CanManageStudents => IsInAnyRole(ApplicationRoles.StudentManagement);

    public bool CanViewStudentRecords => IsInAnyRole(ApplicationRoles.StudentRecords);

    public bool CanManageProfessionals => IsInAnyRole(ApplicationRoles.ProfessionalManagement);

    public bool CanManageEad => IsInAnyRole(ApplicationRoles.CourseManagement);

    public bool CanManageFinance => IsInAnyRole(ApplicationRoles.FinancialManagement);

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
