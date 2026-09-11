using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Staff.DTOs;
public record GetStaffByIdQuery(long Id) : IQuery<StaffResponse>;