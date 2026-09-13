using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;

public record UpdateStudentDocumentCommand(UpdateStudentDocumentRequest Request) : ICommand<StudentDocumentResponse>;