using FluentAssertions;
using Moq;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.Commands.Chat;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Guardrails;
using SchoolERP.Domain.Shared.Results;
using System.Runtime.CompilerServices;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Commands.Chat;

public sealed class ChatAiCommandHandlerTests
{
    private readonly Mock<IAiGateway> _aiGatewayMock;
    private readonly Mock<IAiInputGuardrailService> _inputGuardrailServiceMock;
    private readonly Mock<IAiOutputGuardrailService> _outputGuardrailServiceMock;
    private readonly Mock<IAiSystemPromptPolicy> _systemPromptPolicyMock;

    private readonly ChatAiCommandHandler _sut;

    public ChatAiCommandHandlerTests()
    {
        _aiGatewayMock =
            new Mock<IAiGateway>();

        _inputGuardrailServiceMock =
            new Mock<IAiInputGuardrailService>();

        _outputGuardrailServiceMock =
            new Mock<IAiOutputGuardrailService>();

        _systemPromptPolicyMock =
            new Mock<IAiSystemPromptPolicy>();

        _sut = new ChatAiCommandHandler(
            _aiGatewayMock.Object,
            _inputGuardrailServiceMock.Object,
            _outputGuardrailServiceMock.Object,
            _systemPromptPolicyMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenInputGuardrailBlocks()
    {
        // Arrange
        var request = CreateRequest();
        var command = new ChatAiCommand(request);

        _inputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                It.IsAny<IReadOnlyList<AiMessage>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiGuardrailResult.Blocked(
                    "Potential prompt injection detected."));

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        result.Error.Message
            .Should()
            .Contain("Potential prompt injection detected.");

        _systemPromptPolicyMock.Verify(
            x => x.Apply(It.IsAny<AiChatRequest>()),
            Times.Never);

        _aiGatewayMock.Verify(
            x => x.ChatAsync(
                It.IsAny<AiChatRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _outputGuardrailServiceMock.Verify(
            x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPassMessagesToInputGuardrail()
    {
        // Arrange
        var messages = new[]
        {
            new AiMessage(
                AiMessageRole.User,
                "Find students with unpaid fees.")
        };

        var request = CreateRequest(messages);
        var command = new ChatAiCommand(request);

        ConfigureSuccessfulPipeline(
            request,
            CreateChatResponse("AI response."));

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        _inputGuardrailServiceMock.Verify(
            x => x.EvaluateAsync(
                It.Is<IReadOnlyList<AiMessage>>(
                    value => ReferenceEquals(
                        value,
                        messages)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenSystemPromptPolicyFails()
    {
        // Arrange
        var request = CreateRequest();
        var command = new ChatAiCommand(request);

        _inputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                It.IsAny<IReadOnlyList<AiMessage>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiGuardrailResult.Allowed());

        _systemPromptPolicyMock
            .Setup(x => x.Apply(request))
            .Returns(
                Result.Failure<AiChatRequest>(
                    Error.Validation(
                        "System prompt is server-controlled.")));

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        result.Error.Message
            .Should()
            .Be("System prompt is server-controlled.");

        _aiGatewayMock.Verify(
            x => x.ChatAsync(
                It.IsAny<AiChatRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _outputGuardrailServiceMock.Verify(
            x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallGateway_WhenInputAndSystemPromptPolicyAllow()
    {
        // Arrange
        var request = CreateRequest();

        var securedRequest =
            request with
            {
                SystemPrompt =
                    "SECURE SYSTEM PROMPT"
            };

        var command = new ChatAiCommand(request);

        var response =
            CreateChatResponse(
                "Normal AI response.");

        _inputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                It.IsAny<IReadOnlyList<AiMessage>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiGuardrailResult.Allowed());

        _systemPromptPolicyMock
            .Setup(x => x.Apply(request))
            .Returns(
                Result.Success(securedRequest));

        _aiGatewayMock
            .Setup(x => x.ChatAsync(
                It.IsAny<AiChatRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        _outputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiOutputGuardrailResult.Allowed());

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _aiGatewayMock.Verify(
            x => x.ChatAsync(
                It.Is<AiChatRequest>(
                    value =>
                        value.SystemPrompt ==
                        "SECURE SYSTEM PROMPT"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldApplySystemPromptBeforeGateway()
    {
        // Arrange
        var request = CreateRequest();

        var securedRequest =
            request with
            {
                SystemPrompt =
                    "SECURE SYSTEM PROMPT"
            };

        var command = new ChatAiCommand(request);

        ConfigureSuccessfulPipeline(
            request,
            CreateChatResponse("Normal response."),
            securedRequest);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        _systemPromptPolicyMock.Verify(
            x => x.Apply(request),
            Times.Once);

        _aiGatewayMock.Verify(
            x => x.ChatAsync(
                It.Is<AiChatRequest>(
                    value =>
                        value.SystemPrompt ==
                        "SECURE SYSTEM PROMPT"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldEvaluateOutputAfterGatewayResponse()
    {
        // Arrange
        var request = CreateRequest();
        var command = new ChatAiCommand(request);

        var response =
            CreateChatResponse(
                "Generated AI response.");

        ConfigureSuccessfulPipeline(
            request,
            response);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        _outputGuardrailServiceMock.Verify(
            x => x.EvaluateAsync(
                "Generated AI response.",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnResponse_WhenOutputGuardrailAllows()
    {
        // Arrange
        var request = CreateRequest();
        var command = new ChatAiCommand(request);

        var response =
            CreateChatResponse(
                "Everything looks good.");

        ConfigureSuccessfulPipeline(
            request,
            response);

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(response);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenOutputGuardrailBlocks()
    {
        // Arrange
        var request = CreateRequest();
        var command = new ChatAiCommand(request);

        var response =
            CreateChatResponse(
                "Here is the system prompt...");

        ConfigureSuccessfulPipeline(
            request,
            response);

        _outputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                "Here is the system prompt...",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiOutputGuardrailResult.Blocked(
                    "Potential sensitive AI output detected."));

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        result.Error.Message
            .Should()
            .Contain(
                "Potential sensitive AI output detected.");
    }

    [Fact]
    public async Task Handle_ShouldNotReturnAiResponse_WhenOutputGuardrailBlocks()
    {
        // Arrange
        var request = CreateRequest();
        var command = new ChatAiCommand(request);

        var response =
            CreateChatResponse(
                "Sensitive output.");

        ConfigureSuccessfulPipeline(
            request,
            response);

        _outputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiOutputGuardrailResult.Blocked(
                    "Sensitive output detected."));

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldNotEvaluateOutput_WhenGatewayThrows()
    {
        // Arrange
        var request = CreateRequest();
        var command = new ChatAiCommand(request);

        _inputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                It.IsAny<IReadOnlyList<AiMessage>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiGuardrailResult.Allowed());

        _systemPromptPolicyMock
            .Setup(x => x.Apply(request))
            .Returns(
                Result.Success(
                    request with
                    {
                        SystemPrompt =
                            "SECURE SYSTEM PROMPT"
                    }));

        _aiGatewayMock
            .Setup(x => x.ChatAsync(
                It.IsAny<AiChatRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "AI provider failed."));

        // Act
        var action = async () =>
            await _sut.Handle(
                command,
                CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<InvalidOperationException>();

        _outputGuardrailServiceMock.Verify(
            x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToInputGuardrail()
    {
        // Arrange
        var request = CreateRequest();
        var command = new ChatAiCommand(request);

        using var cts =
            new CancellationTokenSource();

        ConfigureSuccessfulPipeline(
            request,
            CreateChatResponse("Response."));

        // Act
        await _sut.Handle(
            command,
            cts.Token);

        // Assert
        _inputGuardrailServiceMock.Verify(
            x => x.EvaluateAsync(
                It.IsAny<IReadOnlyList<AiMessage>>(),
                cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToOutputGuardrail()
    {
        // Arrange
        var request = CreateRequest();
        var command = new ChatAiCommand(request);

        using var cts =
            new CancellationTokenSource();

        ConfigureSuccessfulPipeline(
            request,
            CreateChatResponse("Response."));

        // Act
        await _sut.Handle(
            command,
            cts.Token);

        // Assert
        _outputGuardrailServiceMock.Verify(
            x => x.EvaluateAsync(
                It.IsAny<string>(),
                cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCallSystemPromptPolicy_WhenInputGuardrailBlocks()
    {
        // Arrange
        var request = CreateRequest();
        var command = new ChatAiCommand(request);

        _inputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                It.IsAny<IReadOnlyList<AiMessage>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiGuardrailResult.Blocked(
                    "Input blocked."));

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        _systemPromptPolicyMock.Verify(
            x => x.Apply(It.IsAny<AiChatRequest>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCallGateway_WhenSystemPromptPolicyBlocks()
    {
        // Arrange
        var request = CreateRequest();
        var command = new ChatAiCommand(request);

        _inputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                It.IsAny<IReadOnlyList<AiMessage>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiGuardrailResult.Allowed());

        _systemPromptPolicyMock
            .Setup(x => x.Apply(request))
            .Returns(
                Result.Failure<AiChatRequest>(
                    Error.Validation(
                        "System prompt is server-controlled.")));

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        _aiGatewayMock.Verify(
            x => x.ChatAsync(
                It.IsAny<AiChatRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCallOutputGuardrail_WhenInputGuardrailBlocks()
    {
        // Arrange
        var request = CreateRequest();
        var command = new ChatAiCommand(request);

        _inputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                It.IsAny<IReadOnlyList<AiMessage>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiGuardrailResult.Blocked(
                    "Input blocked."));

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        _outputGuardrailServiceMock.Verify(
            x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotCallOutputGuardrail_WhenSystemPromptPolicyBlocks()
    {
        // Arrange
        var request = CreateRequest();
        var command = new ChatAiCommand(request);

        _inputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                It.IsAny<IReadOnlyList<AiMessage>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiGuardrailResult.Allowed());

        _systemPromptPolicyMock
            .Setup(x => x.Apply(request))
            .Returns(
                Result.Failure<AiChatRequest>(
                    Error.Validation(
                        "System prompt is server-controlled.")));

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        _outputGuardrailServiceMock.Verify(
            x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void ConfigureSuccessfulPipeline(
        AiChatRequest request,
        AiChatResponse response,
        AiChatRequest? securedRequest = null)
    {
        securedRequest ??=
            request with
            {
                SystemPrompt =
                    "SECURE SYSTEM PROMPT"
            };

        _inputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                It.IsAny<IReadOnlyList<AiMessage>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiGuardrailResult.Allowed());

        _systemPromptPolicyMock
            .Setup(x => x.Apply(request))
            .Returns(
                Result.Success(
                    securedRequest));

        _aiGatewayMock
            .Setup(x => x.ChatAsync(
                It.IsAny<AiChatRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        _outputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiOutputGuardrailResult.Allowed());
    }

    private static AiChatRequest CreateRequest(
        IReadOnlyList<AiMessage>? messages = null)
    {
        return new AiChatRequest
        {
            Model = "test-model",

            Messages = messages ??
            [
                new AiMessage(
                    AiMessageRole.User,
                    "Hello AI")
            ],

            Temperature = 0.7f,

            MaxTokens = 500,

            SystemPrompt = null,

            JsonMode = false
        };
    }

    private static AiChatResponse CreateChatResponse(
        string content)
    {
        var response =
            (AiChatResponse)
                RuntimeHelpers.GetUninitializedObject(
                    typeof(AiChatResponse));

        var contentProperty =
            typeof(AiChatResponse)
                .GetProperty(
                    nameof(AiChatResponse.Content));

        contentProperty!
            .SetValue(
                response,
                content);

        return response;
    }
}