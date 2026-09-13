using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
public record GetStudentByIdQuery(long Id) : IQuery<StudentResponse>;
