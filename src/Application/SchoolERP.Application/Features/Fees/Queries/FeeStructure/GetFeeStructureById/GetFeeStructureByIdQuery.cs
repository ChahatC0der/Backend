using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.FeeStructure.GetFeeStructureById;

public sealed record GetFeeStructureByIdQuery(long Id) : IQuery<FeeStructureResponse>;
