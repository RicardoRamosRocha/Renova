using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Renova.API.DTOs;
using Renova.Infrastructure.Identity;

namespace Renova.API.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    [Authorize(Policy = "UserManagement")]
    public async Task<IActionResult> GetAll()
    {
        var users = _userManager.Users
            .OrderBy(user => user.FullName)
            .ToList();

        var response = new List<UserResponse>();

        foreach (var user in users)
        {
            response.Add(await ToResponse(user));
        }

        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "UserManagement")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(await ToResponse(user));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        var hasUsers = _userManager.Users.Any();

        if (hasUsers && !User.IsInRole(ApplicationRoles.Administrator))
        {
            return Forbid();
        }

        if (!hasUsers && request.Role != ApplicationRoles.Administrator)
        {
            return BadRequest("The first user must be an administrator.");
        }

        if (!ApplicationRoles.All.Contains(request.Role))
        {
            return BadRequest("Invalid role.");
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            return BadRequest("Role is not available in the database.");
        }

        var user = new ApplicationUser
        {
            FullName = request.FullName.Trim(),
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            return BadRequest(createResult.Errors.Select(error => error.Description));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);

        if (!roleResult.Succeeded)
        {
            return BadRequest(roleResult.Errors.Select(error => error.Description));
        }

        return Created($"/users/{user.Id}", await ToResponse(user));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Policy = "UserManagement")]
    public async Task<IActionResult> UpdateStatus(string id, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(error => error.Description));
        }

        return Ok(await ToResponse(user));
    }

    private async Task<UserResponse> ToResponse(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.IsActive,
            roles.ToArray(),
            user.CreatedAt,
            user.UpdatedAt);
    }
}
