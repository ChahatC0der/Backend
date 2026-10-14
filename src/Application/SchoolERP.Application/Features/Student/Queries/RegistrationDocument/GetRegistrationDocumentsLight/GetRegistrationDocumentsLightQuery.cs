using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.RegistrationDocument.GetRegistrationDocumentsLight;

public record GetRegistrationDocumentsLightQuery(long RegistrationId) : IQuery<List<RegistrationDocumentLightResponse>>;
