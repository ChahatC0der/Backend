using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;

public record GetMasterCategoryByIdQuery(long Id) : IQuery<MasterCategoryResponse>;