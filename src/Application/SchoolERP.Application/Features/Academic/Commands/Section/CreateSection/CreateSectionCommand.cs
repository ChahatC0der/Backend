using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.Section.CreateSection;

public record CreateSectionCommand(CreateSectionRequest Request) : ICommand<SectionResponse>;