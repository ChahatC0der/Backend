using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Staff.Queries.StaffDocument.GetStaffDocuments;

public record GetStaffDocumentsQuery(PagedRequest Request, long? StaffId = null) : IQuery<PagedResponse<StaffDocumentResponse>>;
