using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record GetSectionByIdQuery(long Id) : IQuery<SectionResponse>;