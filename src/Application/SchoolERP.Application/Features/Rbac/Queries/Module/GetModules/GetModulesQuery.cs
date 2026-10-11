using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Queries.Module.GetModules;

public record GetModulesQuery(PagedRequest Request) : IQuery<PagedResponse<ModuleResponse>>;