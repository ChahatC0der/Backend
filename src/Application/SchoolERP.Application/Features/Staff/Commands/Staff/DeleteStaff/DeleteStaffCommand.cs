using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Staff.Commands.Staff.DeleteStaff;

public record DeleteStaffCommand(long Id) : ICommand<bool>;
