using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.Student.DeleteStudent;

public record DeleteStudentCommand(long Id) : ICommand<bool>;
