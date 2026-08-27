using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

public class UserRoleConfiguration : BaseTenantEntityConfiguration<UserRole>
{
    public override void Configure(EntityTypeBuilder<UserRole> builder)
    {
        base.Configure(builder);
        builder.ToTable("UserRoles");
        builder.Property(ur => ur.UserId).IsRequired();
        builder.Property(ur => ur.RoleId).IsRequired();
        builder.Property(ur => ur.ScopeType).HasMaxLength(50).IsRequired();
        builder.Property(ur => ur.ScopeValue).HasMaxLength(50).IsRequired(false);
        builder.Property(ur => ur.ValidFrom).HasColumnType("date").IsRequired();
        builder.Property(ur => ur.ValidTo).HasColumnType("date").IsRequired(false);
        builder.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId, ur.ScopeType, ur.ScopeValue }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}