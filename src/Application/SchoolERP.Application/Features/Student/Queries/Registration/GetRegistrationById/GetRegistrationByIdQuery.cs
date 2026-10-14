using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.Registration.GetRegistrationById;

public record GetRegistrationByIdQuery(long Id) : IQuery<RegistrationResponse>;
