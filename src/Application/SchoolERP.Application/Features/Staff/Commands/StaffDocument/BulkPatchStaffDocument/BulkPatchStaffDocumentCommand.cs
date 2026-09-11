using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.BulkPatchStaffDocument;

public record BulkPatchStaffDocumentCommand(BulkPatchStaffDocumentRequest Request) : ICommand<bool>;