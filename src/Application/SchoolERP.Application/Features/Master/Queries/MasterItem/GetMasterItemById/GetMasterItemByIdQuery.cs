using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;

public record GetMasterItemByIdQuery(long Id) : IQuery<MasterItemResponse>;