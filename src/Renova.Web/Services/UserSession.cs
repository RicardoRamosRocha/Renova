using Renova.Infrastructure.Identity;

namespace Renova.Web.Services;

public class UserSession
{
    public ApplicationUser? User { get; private set; }

    public IReadOnlyCollection<string> Roles { get; private set; } = [];

    public bool IsAuthenticated => User is not null;

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
