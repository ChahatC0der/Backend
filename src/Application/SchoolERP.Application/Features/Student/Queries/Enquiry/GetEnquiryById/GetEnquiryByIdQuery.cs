using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.Enquiry.GetEnquiryById;

public record GetEnquiryByIdQuery(long Id) : IQuery<EnquiryResponse>;
