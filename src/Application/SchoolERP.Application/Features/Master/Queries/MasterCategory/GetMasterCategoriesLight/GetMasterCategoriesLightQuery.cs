using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;

public record GetMasterCategoriesLightQuery : IQuery<List<MasterCategoryLightResponse>>;