using Mapster;
using SchoolERP.Application.Features.Student.DTOs;
using AlumniEntity = SchoolERP.Domain.Student.Alumni.Entities.Alumni;

namespace SchoolERP.Application.Features.Student.Mappings;

public class AlumniMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ==================== Alumni ====================
        config.NewConfig<CreateAlumniRequest, AlumniEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.AlumniId)
            .Ignore(dest => dest.LastInteractionDate)
            .Ignore(dest => dest.IsDeceased)
            .Ignore(dest => dest.IsArchived)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Donations)
            .Ignore(dest => dest.SuccessStories)
            .Ignore(dest => dest.Referrals)
            .Ignore(dest => dest.EventRegistrations);

        config.NewConfig<UpdateAlumniRequest, AlumniEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.AlumniId)
            .Ignore(dest => dest.StudentId)
            .Ignore(dest => dest.LastInteractionDate)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Donations)
            .Ignore(dest => dest.SuccessStories)
            .Ignore(dest => dest.Referrals)
            .Ignore(dest => dest.EventRegistrations);

        config.NewConfig<PatchAlumniRequest, AlumniEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.AlumniId)
            .Ignore(dest => dest.StudentId)
            .Ignore(dest => dest.LastInteractionDate)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Donations)
            .Ignore(dest => dest.SuccessStories)
            .Ignore(dest => dest.Referrals)
            .Ignore(dest => dest.EventRegistrations);

        // Alumni -> Response (computed fields filled in handler)
        config.NewConfig<AlumniEntity, AlumniResponse>()
            .Map(dest => dest.FullName,
                src => (src.FirstName + " " + (src.LastName ?? "")).Trim())
            .Map(dest => dest.LastClassName, src => (string?)null)
            .Map(dest => dest.PassingAcademicYearName, src => (string?)null);

        config.NewConfig<AlumniEntity, AlumniLightResponse>()
            .Map(dest => dest.FullName,
                src => (src.FirstName + " " + (src.LastName ?? "")).Trim());
    }
}
