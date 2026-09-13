using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;

public record BulkUpdateStudentDocumentCommand(BulkUpdateStudentDocumentRequest Request) : ICommand<bool>;