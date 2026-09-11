using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.RestoreStaffDocument;

public record RestoreStaffDocumentCommand(long Id) : ICommand<bool>;