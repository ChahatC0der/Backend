using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SchoolERP.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.Status != "active" || user.DeletedAt != null)
            return Unauthorized(new { error = "Invalid email or password." });

        // 2. Verify password using Identity
        var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Unauthorized(new { error = "Invalid email or password." });

        // 3. 🔥 Generate JWT Token (Manual)
        var token = GenerateJwtToken(user);

        return Ok(new
        {
            token = token,
            userId = user.Id,
            name = user.Name,
            email = user.Email,
            tenantId = user.TenantId
        });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email!),
        new Claim("TenantId", user.TenantId.ToString()),
        new Claim("PermissionsVersion", user.PermissionsVersion.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        // 🔥 PHASE 5: Fetch permissions from DB (For testing, hardcode admin permissions)
        // Actual implementation: Get from RolePermissions table via Dapper or EF
        if (user.IsPlatformAdmin || user.Email == "admin@school.com")
        {
            claims.Add(new Claim("permission", "tenant.read"));
            claims.Add(new Claim("permission", "tenant.create"));
            claims.Add(new Claim("permission", "student.read"));
            claims.Add(new Claim("permission", "student.create"));
            claims.Add(new Claim("permission", "student.update"));
            claims.Add(new Claim("permission", "student.delete"));
            // Add all permissions for admin
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequest(string Email, string Password);