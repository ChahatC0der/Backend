using FluentAssertions;
using FluentValidation.TestHelper;
using SchoolERP.Application.Features.AI.Commands.Confirmation;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Commands.Confirmation;

public sealed class ConfirmAiActionCommandValidatorTests
{
    private readonly ConfirmAiActionCommandValidator _validator =
        new();

    [Fact]
    public void Should_pass_when_confirmation_token_is_present()
    {
        var command =
            new ConfirmAiActionCommand(
                "protected-confirmation-token");

        var result =
            _validator.TestValidate(command);

        result.IsValid
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Should_fail_when_confirmation_token_is_empty()
    {
        var command =
            new ConfirmAiActionCommand(
                string.Empty);

        var result =
            _validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(
                x => x.ConfirmationToken)
            .WithErrorMessage(
                "Confirmation token is required.");
    }

    [Fact]
    public void Should_fail_when_confirmation_token_is_whitespace()
    {
        var command =
            new ConfirmAiActionCommand(
                "   ");

        var result =
            _validator.TestValidate(command);

        result
            .ShouldHaveValidationErrorFor(
                x => x.ConfirmationToken);
    }
}