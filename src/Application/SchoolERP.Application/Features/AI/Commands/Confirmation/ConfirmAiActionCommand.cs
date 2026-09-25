using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Features.AI.Commands.Confirmation;

public sealed record ConfirmAiActionCommand(
    string ConfirmationToken)
    : ICommand<AiToolExecutionResult>;