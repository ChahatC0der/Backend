using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Staff.Commands.Department.DeleteDepartment;

public record DeleteDepartmentCommand(long Id) : ICommand<bool>;
