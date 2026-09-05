using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record UpdateClassCommand(UpdateClassRequest Request) : ICommand<ClassResponse>;