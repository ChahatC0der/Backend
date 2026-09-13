using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;

public record BulkDeleteStudentDocumentCommand(BulkDeleteStudentDocumentRequest Request) : ICommand<bool>;