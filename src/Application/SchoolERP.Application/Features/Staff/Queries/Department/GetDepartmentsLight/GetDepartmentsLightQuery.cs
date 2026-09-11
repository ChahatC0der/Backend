using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Staff.DTOs;
public record GetDepartmentsLightQuery : IQuery<List<DepartmentLightResponse>>;