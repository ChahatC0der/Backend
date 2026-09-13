using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
public record GetStudentDocumentsLightQuery(long StudentId) : IQuery<List<StudentDocumentLightResponse>>;
