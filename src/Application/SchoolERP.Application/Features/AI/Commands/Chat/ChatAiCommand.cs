using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Features.AI.Commands.Chat;

public sealed record ChatAiCommand(
    AiChatRequest Request)
    : ICommand<AiChatResponse>;