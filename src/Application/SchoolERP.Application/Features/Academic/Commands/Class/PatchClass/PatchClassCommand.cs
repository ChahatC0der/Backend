using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record PatchClassCommand(PatchClassRequest Request) : ICommand<ClassResponse>;