using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
public record GetStudentEnrollmentsLightQuery(long StudentId) : IQuery<List<StudentEnrollmentLightResponse>>;
