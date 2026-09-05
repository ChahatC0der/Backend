using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Master.Queries.MasterItem.GetMasterItems;

public record GetMasterItemsQuery(PagedRequest Request, long? CategoryId = null) : IQuery<PagedResponse<MasterItemResponse>>;