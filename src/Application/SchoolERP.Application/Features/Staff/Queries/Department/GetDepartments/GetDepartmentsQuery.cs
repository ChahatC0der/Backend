using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Staff.Queries.Department.GetDepartments;

public record GetDepartmentsQuery(PagedRequest Request) : IQuery<PagedResponse<DepartmentResponse>>;
