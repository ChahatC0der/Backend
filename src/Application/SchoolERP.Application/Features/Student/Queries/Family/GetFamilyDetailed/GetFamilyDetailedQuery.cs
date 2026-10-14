using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.Family.GetFamilyDetailed;

public record GetFamilyDetailedQuery(long Id) : IQuery<FamilyDetailedResponse>;
