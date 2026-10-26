using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.AllocateConcession;

public record AllocateConcessionCommand(AllocateConcessionRequest Request) : ICommand<AllocateConcessionResult>;

public sealed record AllocateConcessionResult(int Allocated, int Skipped);
