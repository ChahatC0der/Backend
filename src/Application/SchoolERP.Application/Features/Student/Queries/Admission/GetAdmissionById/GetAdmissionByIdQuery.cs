using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.Admission.GetAdmissionById;

public record GetAdmissionByIdQuery(long Id) : IQuery<AdmissionResponse>;
