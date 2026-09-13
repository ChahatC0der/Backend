using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;

public record PatchStudentDocumentCommand(PatchStudentDocumentRequest Request) : ICommand<StudentDocumentResponse>;