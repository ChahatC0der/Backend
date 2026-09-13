using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
public record GetStudentEnrollmentByIdQuery(long Id) : IQuery<StudentEnrollmentResponse>;
