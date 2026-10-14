using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.RegistrationDocument.UpdateRegistrationDocument;

public record UpdateRegistrationDocumentCommand(UpdateRegistrationDocumentRequest Request) : ICommand<RegistrationDocumentResponse>;
