using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record UpdateSectionCommand(UpdateSectionRequest Request) : ICommand<SectionResponse>;