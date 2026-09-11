using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Staff.Commands.Department.RestoreDepartment;

public record RestoreDepartmentCommand(long Id) : ICommand<bool>;
