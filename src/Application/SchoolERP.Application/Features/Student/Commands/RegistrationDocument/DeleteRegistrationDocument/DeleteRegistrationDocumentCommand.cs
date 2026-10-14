using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.RegistrationDocument.DeleteRegistrationDocument;

public record DeleteRegistrationDocumentCommand(long Id) : ICommand<bool>;
