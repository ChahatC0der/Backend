using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;

public record GetMasterItemsLightQuery(long CategoryId) : IQuery<List<MasterItemLightResponse>>;