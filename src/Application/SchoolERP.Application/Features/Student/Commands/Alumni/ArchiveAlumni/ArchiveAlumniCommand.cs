using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.Alumni.ArchiveAlumni;

public record ArchiveAlumniCommand(long Id, bool IsDeceased = false) : ICommand<AlumniResponse>;
