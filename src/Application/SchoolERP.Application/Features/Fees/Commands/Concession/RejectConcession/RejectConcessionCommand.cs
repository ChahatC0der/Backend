using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.RejectConcession;

public record RejectConcessionCommand(long AllocationId, RejectConcessionRequest Request) : ICommand<ConcessionAllocationResponse>;
