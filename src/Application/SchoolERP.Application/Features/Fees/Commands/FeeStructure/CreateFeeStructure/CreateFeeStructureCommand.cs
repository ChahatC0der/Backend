using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.CreateFeeStructure;

public record CreateFeeStructureCommand(CreateFeeStructureRequest Request) : ICommand<FeeStructureResponse>;
