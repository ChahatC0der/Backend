using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record GetClassByIdQuery(long Id) : IQuery<ClassResponse>;