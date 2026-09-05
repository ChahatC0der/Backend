using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record GetClassGroupByIdQuery(long Id) : IQuery<ClassGroupResponse>;