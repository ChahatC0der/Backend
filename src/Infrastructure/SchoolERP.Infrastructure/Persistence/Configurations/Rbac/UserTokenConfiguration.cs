using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

public class UserTokenConfiguration : BaseTenantEntityConfiguration<UserToken>
{
    public override void Configure(EntityTypeBuilder<UserToken> builder)
    {
        base.Configure(builder);
        builder.ToTable("UserTokens");
        builder.Property(ut => ut.UserId).IsRequired();
        builder.Property(ut => ut.TokenType).HasMaxLength(50).IsRequired();
        builder.Property(ut => ut.Token).HasMaxLength(255).IsRequired();
        builder.HasOne(ut => ut.User).WithMany(u => u.UserTokens).HasForeignKey(ut => ut.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(ut => ut.Token).IsUnique();
    }
}