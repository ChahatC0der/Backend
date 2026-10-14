using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.EnquiryFollowup.GetEnquiryFollowupById;

public record GetEnquiryFollowupByIdQuery(long Id) : IQuery<EnquiryFollowupResponse>;
