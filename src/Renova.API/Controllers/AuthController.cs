using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Renova.API.Auth;
using Renova.API.DTOs;
using Renova.Infrastructure.Identity;

namespace Renova.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AccessTokenService _accessTokenService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(
        AccessTokenService accessTokenService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _accessTokenService = accessTokenService;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());

        if (user is null || !user.IsActive)
        {
            return Unauthorized();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _accessTokenService.CreateToken(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            roles.ToArray());

        return Ok(new AuthResponse(
            token,
            "Bearer",
            DateTime.UtcNow.AddHours(8),
            new UserResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.IsActive,
                roles.ToArray(),
                user.CreatedAt,
                user.UpdatedAt)));
    }
}
