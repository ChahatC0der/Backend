using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.Alumni.DeleteAlumni;

public record DeleteAlumniCommand(long Id) : ICommand<bool>;
