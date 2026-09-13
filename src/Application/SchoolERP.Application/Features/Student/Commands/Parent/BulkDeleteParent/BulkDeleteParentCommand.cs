using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;

public record BulkDeleteParentCommand(BulkDeleteParentRequest Request) : ICommand<bool>;