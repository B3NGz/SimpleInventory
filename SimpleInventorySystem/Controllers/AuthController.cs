using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleInventorySystem.Data;
using SimpleInventorySystem.Model.Entities;
using SimpleInventorySystem.Service;


namespace SimpleInventorySystem.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwtService;

    public AuthController(
        ApplicationDbContext context,
        JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    // ============================================
    // GOOGLE LOGIN
    // ============================================

    [HttpGet("google")]
    [AllowAnonymous]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(
                nameof(GoogleCallback),
                "Auth")
        };

        return Challenge(
            properties,
            GoogleDefaults.AuthenticationScheme);
    }

    // ============================================
    // GOOGLE CALLBACK
    // ============================================

    [HttpGet("google-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback()
    {
        var authenticateResult =
            await HttpContext.AuthenticateAsync("External");

        if (!authenticateResult.Succeeded)
        {
            return Unauthorized(new
            {
                message = "Google authentication failed."
            });
        }

        var claims = authenticateResult.Principal?.Claims;

        if (claims == null)
        {
            return Unauthorized(new
            {
                message = "Google user information was not found."
            });
        }

        var googleSubjectId =
            claims.FirstOrDefault(
                x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        var email =
            claims.FirstOrDefault(
                x => x.Type == ClaimTypes.Email)?.Value;

        var fullName =
            claims.FirstOrDefault(
                x => x.Type == ClaimTypes.Name)?.Value;

        if (string.IsNullOrWhiteSpace(googleSubjectId) ||
            string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new
            {
                message = "Required Google account information is missing."
            });
        }

        // ============================================
        // FIND USER
        // ============================================

        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.GoogleSubjectId == googleSubjectId);

        // ============================================
        // CREATE USER IF NEW
        // ============================================

        if (user == null)
        {
            user = new User
            {
                FullName = fullName ?? email,
                Email = email,
                GoogleSubjectId = googleSubjectId,
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();
        }

        // ============================================
        // CHECK ACCOUNT
        // ============================================

        if (!user.IsActive)
        {
            return Unauthorized(new
            {
                message = "Your account is disabled."
            });
        }

        // ============================================
        // GENERATE JWT
        // ============================================

        var token = _jwtService.GenerateToken(user);

        // ============================================
        // SUCCESS
        // ============================================

        return Ok(new
        {
            message = "Google authentication successful.",
            token = token,
            user = new
            {
                user.UserId,
                user.FullName,
                user.Email,
                user.Role,
                user.IsActive
            }
        });
    }

    // ============================================
    // LOGOUT
    // ============================================

    [HttpGet("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(new
        {
            message = "Logout successful. Remove the JWT from the client."
        });
    }
}