using SchoolERP.Application.Features.Rbac.DTOs;

namespace SchoolERP.Application.Features.Auth.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, UserResponse User);