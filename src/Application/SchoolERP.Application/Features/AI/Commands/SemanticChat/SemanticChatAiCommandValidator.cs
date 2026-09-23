using FluentValidation;

namespace SchoolERP.Application.Features.AI.Commands.SemanticChat;

public sealed class SemanticChatAiCommandValidator
    : AbstractValidator<SemanticChatAiCommand>
{
    public SemanticChatAiCommandValidator()
    {
        RuleFor(x => x.Request.Model)
            .NotEmpty();

        RuleFor(x => x.Request.Messages)
            .NotEmpty();

        RuleForEach(x => x.Request.Messages)
            .ChildRules(message =>
            {
                message.RuleFor(x => x.Content)
                    .NotEmpty();

                message.RuleFor(x => x.Role)
                    .IsInEnum();
            });
    }
}