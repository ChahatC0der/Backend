using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.Admission.RestoreAdmission;

public record RestoreAdmissionCommand(long Id) : ICommand<bool>;
