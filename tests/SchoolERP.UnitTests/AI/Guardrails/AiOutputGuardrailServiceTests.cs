using FluentAssertions;
using Moq;
using SchoolERP.Application.Features.AI.Guardrails;
using SchoolERP.Infrastructure.Services.AI.Guardrails;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Guardrails;

public sealed class AiOutputGuardrailServiceTests
{
    [Fact]
    public async Task EvaluateAsync_ShouldBlock_WhenContentIsNull()
    {
        // Arrange
        var sut =
            new AiOutputGuardrailService(
                Array.Empty<IAiOutputGuardrail>());

        // Act
        var result =
            await sut.EvaluateAsync(null!);

        // Assert
        result.IsAllowed.Should().BeFalse();

        result.Reasons
            .Should()
            .Contain(
                "AI response content cannot be null.");
    }

    [Fact]
    public async Task EvaluateAsync_ShouldAllow_WhenNoGuardrailsAreRegistered()
    {
        // Arrange
        var sut =
            new AiOutputGuardrailService(
                Array.Empty<IAiOutputGuardrail>());

        // Act
        var result =
            await sut.EvaluateAsync(
                "Normal AI response.");

        // Assert
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_ShouldAllow_WhenAllGuardrailsAllow()
    {
        // Arrange
        var first =
            new Mock<IAiOutputGuardrail>();

        var second =
            new Mock<IAiOutputGuardrail>();

        first
            .Setup(x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiOutputGuardrailResult.Allowed());

        second
            .Setup(x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiOutputGuardrailResult.Allowed());

        var sut =
            new AiOutputGuardrailService(
            [
                first.Object,
                second.Object
            ]);

        // Act
        var result =
            await sut.EvaluateAsync(
                "Normal response.");

        // Assert
        result.IsAllowed.Should().BeTrue();

        first.Verify(
            x => x.EvaluateAsync(
                "Normal response.",
                It.IsAny<CancellationToken>()),
            Times.Once);

        second.Verify(
            x => x.EvaluateAsync(
                "Normal response.",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_ShouldReturnBlockedResult_WhenGuardrailBlocks()
    {
        // Arrange
        var guardrail =
            new Mock<IAiOutputGuardrail>();

        guardrail
            .Setup(x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiOutputGuardrailResult.Blocked(
                    "Sensitive output detected."));

        var sut =
            new AiOutputGuardrailService(
            [
                guardrail.Object
            ]);

        // Act
        var result =
            await sut.EvaluateAsync(
                "Unsafe response.");

        // Assert
        result.IsAllowed.Should().BeFalse();

        result.Reasons
            .Should()
            .Contain(
                "Sensitive output detected.");
    }

    [Fact]
    public async Task EvaluateAsync_ShouldStopAtFirstBlockedGuardrail()
    {
        // Arrange
        var first =
            new Mock<IAiOutputGuardrail>();

        var second =
            new Mock<IAiOutputGuardrail>();

        first
            .Setup(x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiOutputGuardrailResult.Blocked(
                    "Blocked by first guardrail."));

        var sut =
            new AiOutputGuardrailService(
            [
                first.Object,
                second.Object
            ]);

        // Act
        var result =
            await sut.EvaluateAsync(
                "Unsafe response.");

        // Assert
        result.IsAllowed.Should().BeFalse();

        second.Verify(
            x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_ShouldPassSameContentToGuardrails()
    {
        // Arrange
        var guardrail =
            new Mock<IAiOutputGuardrail>();

        guardrail
            .Setup(x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiOutputGuardrailResult.Allowed());

        var sut =
            new AiOutputGuardrailService(
            [
                guardrail.Object
            ]);

        const string content =
            "The student attendance is 92%.";

        // Act
        await sut.EvaluateAsync(content);

        // Assert
        guardrail.Verify(
            x => x.EvaluateAsync(
                content,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_ShouldThrow_WhenCancelled()
    {
        // Arrange
        var guardrail =
            new Mock<IAiOutputGuardrail>();

        var sut =
            new AiOutputGuardrailService(
            [
                guardrail.Object
            ]);

        using var cts =
            new CancellationTokenSource();

        cts.Cancel();

        // Act
        var action = async () =>
            await sut.EvaluateAsync(
                "Response.",
                cts.Token);

        // Assert
        await action
            .Should()
            .ThrowAsync<OperationCanceledException>();
    }
}