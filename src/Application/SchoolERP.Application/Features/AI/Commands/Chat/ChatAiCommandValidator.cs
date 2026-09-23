using FluentValidation;

namespace SchoolERP.Application.Features.AI.Commands.Chat;

public sealed class ChatAiCommandValidator
    : AbstractValidator<ChatAiCommand>
{
    public ChatAiCommandValidator()
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