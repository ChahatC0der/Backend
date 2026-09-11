using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentDocumentEntity = SchoolERP.Domain.Student.Entities.StudentDocument;
using SchoolERP.Domain.Student.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student;

public class StudentDocumentConfiguration : BaseEntityConfiguration<StudentDocumentEntity>
{
    public override void Configure(EntityTypeBuilder<StudentDocumentEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("StudentDocuments");

        builder.Property(x => x.DocumentType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FileUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.Verified).HasDefaultValue(false);

        builder.HasOne(x => x.Student)
            .WithMany(s => s.Documents)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => x.DocumentType);
    }
}
