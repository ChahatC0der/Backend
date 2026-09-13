using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.StudentDocument.GetStudentDocuments;

public record GetStudentDocumentsQuery(PagedRequest Request, long? StudentId = null) : IQuery<PagedResponse<StudentDocumentResponse>>;
