using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.DeleteConcession;

public record DeleteConcessionCommand(long Id) : ICommand<bool>;
