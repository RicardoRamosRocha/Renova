using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Renova.Infrastructure.Identity;

namespace Renova.Web.Services;

public class UserSessionLoader
{
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserSession _session;

    public UserSessionLoader(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        UserSession session)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _session = session;
    }

    public async Task LoadAsync()
    {
        await _loadLock.WaitAsync();

        try
        {
            var principal = _httpContextAccessor.HttpContext?.User;

            if (principal?.Identity?.IsAuthenticated != true)
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

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null || !user.IsActive)
            {
                _session.SignOut();
                return;
            }

            var roles = await _userManager.GetRolesAsync(user);
            _session.SignIn(user, roles.ToArray());
        }
        finally
        {
            _loadLock.Release();
        }
    }
}
