using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;

public record BulkPatchStudentDocumentCommand(BulkPatchStudentDocumentRequest Request) : ICommand<bool>;