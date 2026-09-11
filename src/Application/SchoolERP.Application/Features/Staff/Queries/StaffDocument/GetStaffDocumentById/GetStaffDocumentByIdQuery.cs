using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Staff.DTOs;
public record GetStaffDocumentByIdQuery(long Id) : IQuery<StaffDocumentResponse>;