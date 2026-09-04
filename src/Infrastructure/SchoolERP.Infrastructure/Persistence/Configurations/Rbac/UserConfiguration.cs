using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Rbac.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Rbac;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users"); // ✅ correct table name

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();

        builder.Property(u => u.TenantId).IsRequired(false);
        builder.Property(u => u.BranchId).IsRequired(false);
        builder.Property(u => u.Name).HasMaxLength(255).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(255).IsRequired();
        builder.Property(u => u.Phone).HasMaxLength(50);
        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.IsPlatformAdmin).HasDefaultValue(false);
        builder.Property(u => u.Status).HasMaxLength(50);
        builder.Property(u => u.PermissionsVersion).IsRequired();
        builder.Property(u => u.LastLogin).IsRequired(false);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.TenantId);
    }
}