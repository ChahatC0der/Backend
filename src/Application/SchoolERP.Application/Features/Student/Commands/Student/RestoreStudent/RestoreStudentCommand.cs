using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.Student.RestoreStudent;

public record RestoreStudentCommand(long Id) : ICommand<bool>;
