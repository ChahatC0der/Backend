using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Concession.GetConcessionLights;

public record class GetConcessionLightsQuery : IQuery<List<ConcessionLightResponse>>;