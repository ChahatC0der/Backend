using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
public record GetParentsLightQuery(long StudentId) : IQuery<List<ParentLightResponse>>;
