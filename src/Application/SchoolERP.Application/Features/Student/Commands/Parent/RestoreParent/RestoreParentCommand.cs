using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.Parent.RestoreParent;

public record RestoreParentCommand(long Id) : ICommand<bool>;