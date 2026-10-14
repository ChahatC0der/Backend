using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.Enquiry.ExportEnquiries;

public record ExportEnquiriesQuery : IQuery<byte[]>;
