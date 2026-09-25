using FluentAssertions;
using Moq;
using SchoolERP.Application.Features.AI.Commands.Confirmation;
using SchoolERP.Application.Features.AI.Confirmation;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Domain.Shared.Results;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Commands.Confirmation;

public sealed class ConfirmAiActionCommandHandlerTests
{
    [Fact]
    public async Task Should_forward_confirmation_token_to_execution_service()
    {
        const string token =
            "protected-confirmation-token";

        var expectedResult =
            Result.Success(
                new AiToolExecutionResult
                {
                    ToolName = "DeleteStudent",
                    Output =
                        new
                        {
                            Deleted = true
                        },
                    Message =
                        "Student deleted successfully."
                });

        var service =
            new Mock<IAiConfirmationExecutionService>();

        service
            .Setup(x => x.ExecuteAsync(
                token,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var handler =
            new ConfirmAiActionCommandHandler(
                service.Object);

        var result =
            await handler.Handle(
                new ConfirmAiActionCommand(token),
                CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        result.Value!.ToolName
            .Should()
            .Be("DeleteStudent");

        result.Value.Message
            .Should()
            .Be("Student deleted successfully.");

        service.Verify(
            x => x.ExecuteAsync(
                token,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_forward_execution_failure()
    {
        const string token =
            "already-used-token";

        var expectedResult =
            Result.Failure<AiToolExecutionResult>(
                Error.Unauthorized(
                    "AI confirmation token has already been used or is unavailable."));

        var service =
            new Mock<IAiConfirmationExecutionService>();

        service
            .Setup(x => x.ExecuteAsync(
                token,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var handler =
            new ConfirmAiActionCommandHandler(
                service.Object);

        var result =
            await handler.Handle(
                new ConfirmAiActionCommand(token),
                CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        result.Error.Code
            .Should()
            .Be("Unauthorized");

        result.Error.Message
            .Should()
            .Contain("already been used");

        service.Verify(
            x => x.ExecuteAsync(
                token,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_pass_cancellation_token()
    {
        const string token =
            "protected-confirmation-token";

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        var service =
            new Mock<IAiConfirmationExecutionService>();

        service
            .Setup(x => x.ExecuteAsync(
                token,
                cancellationToken))
            .ReturnsAsync(
                Result.Success(
                    new AiToolExecutionResult
                    {
                        ToolName = "DeleteStudent"
                    }));

        var handler =
            new ConfirmAiActionCommandHandler(
                service.Object);

        await handler.Handle(
            new ConfirmAiActionCommand(token),
            cancellationToken);

        service.Verify(
            x => x.ExecuteAsync(
                token,
                cancellationToken),
            Times.Once);
    }
}