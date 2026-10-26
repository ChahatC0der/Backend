using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.AutoApplySiblingConcessions;

public record AutoApplySiblingConcessionsCommand(long AcademicYearId) : ICommand<AutoApplySiblingResult>;

public sealed record AutoApplySiblingResult(int FamiliesScanned, int SiblingsAllocated, int Skipped);
