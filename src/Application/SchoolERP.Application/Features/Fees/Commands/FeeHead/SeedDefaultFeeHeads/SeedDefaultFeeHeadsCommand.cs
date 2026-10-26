using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.SeedDefaultFeeHeads;

public record SeedDefaultFeeHeadsCommand : ICommand<SeedDefaultsResult>;

public sealed record SeedDefaultsResult(int Inserted, int Skipped, IReadOnlyList<string> InsertedCodes);
