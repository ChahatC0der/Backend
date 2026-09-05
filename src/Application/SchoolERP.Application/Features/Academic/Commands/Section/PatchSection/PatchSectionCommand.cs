using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record PatchSectionCommand(PatchSectionRequest Request) : ICommand<SectionResponse>;