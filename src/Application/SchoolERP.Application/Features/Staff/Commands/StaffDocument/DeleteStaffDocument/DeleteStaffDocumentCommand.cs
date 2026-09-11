using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.DeleteStaffDocument;

public record DeleteStaffDocumentCommand(long Id) : ICommand<bool>;
