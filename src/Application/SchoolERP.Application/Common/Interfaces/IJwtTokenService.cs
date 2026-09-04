using SchoolERP.Domain.Rbac.Entities;

public interface IJwtTokenService
{
    string GenerateToken(User user, IList<string> roles, IList<string> permissions);
}