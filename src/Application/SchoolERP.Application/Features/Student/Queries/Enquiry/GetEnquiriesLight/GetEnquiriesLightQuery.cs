using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.Enquiry.GetEnquiriesLight;

public record GetEnquiriesLightQuery : IQuery<List<EnquiryLightResponse>>;
