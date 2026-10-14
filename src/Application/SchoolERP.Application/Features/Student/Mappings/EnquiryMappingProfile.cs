using Mapster;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Student.Enquiry.Entities;

namespace SchoolERP.Application.Features.Student.Mappings;

public class EnquiryMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ==================== Enquiry ====================
        config.NewConfig<CreateEnquiryRequest, Enquiry>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.EnquiryDate)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.ConvertedToRegistrationId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Followups)
            .Ignore(dest => dest.StatusLogs);

        config.NewConfig<UpdateEnquiryRequest, Enquiry>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.EnquiryDate)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.ConvertedToRegistrationId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Followups)
            .Ignore(dest => dest.StatusLogs);

        config.NewConfig<PatchEnquiryRequest, Enquiry>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.EnquiryDate)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.ConvertedToRegistrationId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Followups)
            .Ignore(dest => dest.StatusLogs);

        // Enquiry -> EnquiryResponse (ClassName, AssignedToName get filled in handler)
        config.NewConfig<Enquiry, EnquiryResponse>()
            .Map(dest => dest.ClassName, src => (string?)null)
            .Map(dest => dest.AssignedToName, src => (string?)null);

        config.NewConfig<Enquiry, EnquiryLightResponse>();

        // ==================== EnquiryFollowup ====================
        config.NewConfig<CreateEnquiryFollowupRequest, EnquiryFollowup>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Enquiry);

        config.NewConfig<UpdateEnquiryFollowupRequest, EnquiryFollowup>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Enquiry);

        config.NewConfig<PatchEnquiryFollowupRequest, EnquiryFollowup>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Enquiry);

        config.NewConfig<EnquiryFollowup, EnquiryFollowupResponse>();
        config.NewConfig<EnquiryFollowup, EnquiryFollowupLightResponse>();
    }
}
