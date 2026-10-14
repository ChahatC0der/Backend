using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.RegistrationDocument.GetRegistrationDocuments;

public record GetRegistrationDocumentsQuery(PagedRequest Request, long? RegistrationId = null) : IQuery<PagedResponse<RegistrationDocumentResponse>>;
