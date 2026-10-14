using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.Family.GetFamilyById;

public record GetFamilyByIdQuery(long Id) : IQuery<FamilyResponse>;
