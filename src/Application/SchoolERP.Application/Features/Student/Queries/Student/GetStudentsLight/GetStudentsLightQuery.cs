using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
public record GetStudentsLightQuery : IQuery<List<StudentLightResponse>>;
