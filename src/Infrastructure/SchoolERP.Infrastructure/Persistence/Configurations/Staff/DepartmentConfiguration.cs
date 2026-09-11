using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Staff.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Staff;

public class DepartmentConfiguration : BaseBranchOnlyEntityConfiguration<Department>
{
    public override void Configure(EntityTypeBuilder<Department> builder)
    {
        base.Configure(builder);

        builder.ToTable("Departments");

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.HodId).IsRequired(false);

        builder.HasIndex(x => new { x.BranchId, x.Code }).IsUnique();

        builder.HasOne(x => x.Hod)
            .WithMany()
            .HasForeignKey(x => x.HodId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}