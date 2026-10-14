using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.Family.DeleteFamily;

public record DeleteFamilyCommand(long Id) : ICommand<bool>;
