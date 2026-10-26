using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.PatchFeeStructureStatus;

public record PatchFeeStructureStatusCommand(long Id, PatchFeeStructureStatusRequest Request) : ICommand<FeeStructureResponse>;
