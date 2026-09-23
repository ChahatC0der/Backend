using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Features.AI.Commands.SemanticChat;

public sealed record SemanticChatAiCommand(
    AiChatRequest Request)
    : ICommand<AiSemanticResponse>;