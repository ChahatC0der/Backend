using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.EnquiryFollowup.GetEnquiryFollowups;

public record GetEnquiryFollowupsQuery(long EnquiryId) : IQuery<List<EnquiryFollowupResponse>>;
