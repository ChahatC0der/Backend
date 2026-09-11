using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Staff.Queries.Staff.GetStaffs;

public record GetStaffsQuery(PagedRequest Request) : IQuery<PagedResponse<StaffResponse>>;
