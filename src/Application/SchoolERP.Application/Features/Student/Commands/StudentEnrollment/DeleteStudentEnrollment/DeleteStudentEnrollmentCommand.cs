using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.DeleteStudentEnrollment;

public record DeleteStudentEnrollmentCommand(long Id) : ICommand<bool>;
