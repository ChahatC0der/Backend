using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.RegistrationDocument.ExportRegistrationDocuments;

public record ExportRegistrationDocumentsQuery(long RegistrationId) : IQuery<byte[]>;
