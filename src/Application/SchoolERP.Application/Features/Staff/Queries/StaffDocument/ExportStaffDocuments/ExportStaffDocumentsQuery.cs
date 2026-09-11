using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Staff.DTOs;
public record ExportStaffDocumentsQuery(long StaffId) : IQuery<byte[]>;