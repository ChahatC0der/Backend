using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.Admission.DeleteAdmission;

public record DeleteAdmissionCommand(long Id) : ICommand<bool>;
