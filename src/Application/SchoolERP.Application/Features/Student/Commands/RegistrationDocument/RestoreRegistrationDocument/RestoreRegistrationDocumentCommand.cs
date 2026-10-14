using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.RegistrationDocument.RestoreRegistrationDocument;

public record RestoreRegistrationDocumentCommand(long Id) : ICommand<bool>;
