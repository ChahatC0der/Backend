using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.AcademicYear.CreateAcademicYear;

public record CreateAcademicYearCommand(CreateAcademicYearRequest Request) : ICommand<AcademicYearResponse>;