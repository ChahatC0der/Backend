using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.Student.GetStudents;

public record GetStudentsQuery(PagedRequest Request) : IQuery<PagedResponse<StudentResponse>>;
