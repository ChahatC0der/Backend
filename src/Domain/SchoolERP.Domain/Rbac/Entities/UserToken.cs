using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Rbac.Entities;

public class UserToken : BaseTenantEntity
{
    public long UserId { get; set; }
    public string TokenType { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public User User { get; set; } = null!;
}