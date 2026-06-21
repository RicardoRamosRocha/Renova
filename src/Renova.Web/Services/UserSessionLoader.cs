using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Renova.Infrastructure.Identity;

namespace Renova.Web.Services;

public class UserSessionLoader
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserSession _session;

    public UserSessionLoader(
        UserManager<ApplicationUser> userManager,
        UserSession session)
    {
        _userManager = userManager;
        _session = session;
    }

    public async Task LoadAsync(Task<AuthenticationState>? authenticationStateTask)
    {
        if (authenticationStateTask is null)
        {
            return;
        }

        var authenticationState = await authenticationStateTask;
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated != true)
        {
            if (_session.IsAuthenticated)
            {
                _session.SignOut();
            }

            return;
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        if (_session.User?.Id == userId)
        {
            return;
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
        {
            _session.SignOut();
            return;
        }

        var roles = await _userManager.GetRolesAsync(user);
        _session.SignIn(user, roles.ToArray());
    }
}
