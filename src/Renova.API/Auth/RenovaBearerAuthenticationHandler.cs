using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Renova.Infrastructure.Identity;

namespace Renova.API.Auth;

public class RenovaBearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AccessTokenService _accessTokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RenovaBearerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AccessTokenService accessTokenService,
        UserManager<ApplicationUser> userManager)
        : base(options, logger, encoder)
    {
        _accessTokenService = accessTokenService;
        _userManager = userManager;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorization) ||
            !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authorization["Bearer ".Length..].Trim();
        var payload = _accessTokenService.ValidateToken(token);

        if (payload is null)
        {
            return AuthenticateResult.Fail("Invalid token.");
        }

        var user = await _userManager.FindByIdAsync(payload.UserId);

        if (user is null || !user.IsActive)
        {
            return AuthenticateResult.Fail("User is inactive or no longer exists.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
