using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.FeeStructure.GetFeeStructuresLight;

public sealed record GetFeeStructuresLightQuery(long? AcademicYearId = null) : IQuery<IReadOnlyList<FeeStructureLightResponse>>;
