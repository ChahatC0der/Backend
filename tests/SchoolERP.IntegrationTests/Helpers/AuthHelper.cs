using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Infrastructure.Identity;
using System.Net.Http.Json;

namespace SchoolERP.IntegrationTests.Helpers;

public class AuthHelper
{
    private readonly IServiceProvider _serviceProvider;

    public AuthHelper(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<string> GetTokenAsync(string email, string password)
    {
        using var scope = _serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Create user if not exists (for testing)
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Name = "Test User",
                TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Status = "active"
            };
            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, "Admin");
        }

        var result = await signInManager.PasswordSignInAsync(user, password, false, false);
        if (!result.Succeeded)
            throw new Exception("Login failed");

        // 🔥 Call the actual login endpoint to get JWT token
        var client = new HttpClient();
        var loginRequest = new { email, password };
        var response = await client.PostAsJsonAsync("http://localhost:5000/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse!.Token;
    }
}

public record LoginResponse(string Token, string Email);