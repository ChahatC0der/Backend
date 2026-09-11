using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Staff.DTOs;
public record GetStaffDocumentsLightQuery(long StaffId) : IQuery<List<StaffDocumentLightResponse>>;