using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Staff.Commands.Staff.RestoreStaff;

public record RestoreStaffCommand(long Id) : ICommand<bool>;
