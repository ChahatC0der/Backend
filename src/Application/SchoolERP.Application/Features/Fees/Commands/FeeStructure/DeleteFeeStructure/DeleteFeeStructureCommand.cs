using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.DeleteFeeStructure;

public record DeleteFeeStructureCommand(long Id) : ICommand<bool>;
