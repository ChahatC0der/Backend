using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Queries.Class.GetClasses;

public record GetClassesQuery(PagedRequest Request) : IQuery<PagedResponse<ClassResponse>>;