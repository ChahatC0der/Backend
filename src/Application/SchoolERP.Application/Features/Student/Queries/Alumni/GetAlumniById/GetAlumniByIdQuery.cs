using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.Alumni.GetAlumniById;

public record GetAlumniByIdQuery(long Id) : IQuery<AlumniResponse>;
