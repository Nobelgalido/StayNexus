using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayNexus.Core.Interfaces;
using StayNexus.Core.Models;
using StayNexus.Shared.Requests;
using StayNexus.Shared.Responses;

namespace StayNexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return BadRequest(new { message = "Email is already registered." });
        }

        var allowedRoles = new[] { "Admin", "Owner", "Guest" };
        if (!allowedRoles.Contains(request.Role))
        {
            return BadRequest(new { message = "Invalid role specified." });
        }

        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { message = "Registration failed.", errors });
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        var token = _tokenService.GenerateToken(user, request.Role);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = request.Role,
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Guest";
        var token = _tokenService.GenerateToken(user, role);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = role,
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        });
    }
}