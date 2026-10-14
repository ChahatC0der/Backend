using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.Alumni.RestoreAlumni;

public record RestoreAlumniCommand(long Id) : ICommand<bool>;
