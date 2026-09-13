using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;

public record CreateStudentDocumentCommand(CreateStudentDocumentRequest Request) : ICommand<StudentDocumentResponse>;