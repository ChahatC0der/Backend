using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Alumni.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Alumni;

public class AlumniEventRegistrationConfiguration : BaseEntityConfiguration<AlumniEventRegistration>
{
    public override void Configure(EntityTypeBuilder<AlumniEventRegistration> builder)
    {
        base.Configure(builder);
        builder.ToTable("AlumniEventRegistrations");

        builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("registered");
        builder.Property(x => x.FeeAmount).HasColumnType("decimal(12,2)");

        builder.HasOne(x => x.Event)
            .WithMany(e => e.Registrations)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Alumni)
            .WithMany(a => a.EventRegistrations)
            .HasForeignKey(x => x.AlumniId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.EventId, x.AlumniId }).IsUnique();
        builder.HasIndex(x => x.AlumniId);
        builder.HasIndex(x => x.Status);

        // Matching filters
        builder.HasQueryFilter(x => !x.Event.IsDeleted && !x.Alumni.IsDeleted);
    }
}
