using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;

public record GetStudentDocumentByIdQuery(long Id) : IQuery<StudentDocumentResponse>;
