using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
public record GetParentByIdQuery(long Id) : IQuery<ParentResponse>;
