using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.CreateConcession;

public record CreateConcessionCommand(CreateConcessionRequest Request) : ICommand<ConcessionResponse>;
