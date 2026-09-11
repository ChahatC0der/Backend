using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.Parent.DeleteParent;

public record DeleteParentCommand(long Id) : ICommand<bool>;
