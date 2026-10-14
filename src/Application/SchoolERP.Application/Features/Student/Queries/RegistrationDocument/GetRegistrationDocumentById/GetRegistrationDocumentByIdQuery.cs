using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.RegistrationDocument.GetRegistrationDocumentById;

public record GetRegistrationDocumentByIdQuery(long Id) : IQuery<RegistrationDocumentResponse>;
