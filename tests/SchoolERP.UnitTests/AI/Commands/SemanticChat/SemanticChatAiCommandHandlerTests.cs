using FluentAssertions;
using Moq;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.Commands.SemanticChat;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Guardrails;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Domain.Shared.Results;
using System.Runtime.CompilerServices;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Commands.SemanticChat;

public sealed class SemanticChatAiCommandHandlerTests
{
    private readonly Mock<IAiGateway> _aiGatewayMock;
    private readonly Mock<IAiInputGuardrailService> _inputGuardrailServiceMock;
    private readonly Mock<IAiOutputGuardrailService> _outputGuardrailServiceMock;
    private readonly Mock<IAiSystemPromptPolicy> _systemPromptPolicyMock;

    private readonly SemanticChatAiCommandHandler _sut;

    public SemanticChatAiCommandHandlerTests()
    {
        _aiGatewayMock =
            new Mock<IAiGateway>();

        _inputGuardrailServiceMock =
            new Mock<IAiInputGuardrailService>();

        _outputGuardrailServiceMock =
            new Mock<IAiOutputGuardrailService>();

        _systemPromptPolicyMock =
            new Mock<IAiSystemPromptPolicy>();

        /*
         * Current production handler uses the concrete
         * AiActionValidationService.
         *
         * These tests deliberately use a semantic "Message"
         * response so that action validation is not reached.
         */
        _sut = new SemanticChatAiCommandHandler(
            _aiGatewayMock.Object,
            _inputGuardrailServiceMock.Object,
            _outputGuardrailServiceMock.Object,
            _systemPromptPolicyMock.Object,
            null!);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenInputGuardrailBlocks()
    {
        // Arrange
        var request = CreateRequest();
        var command = new SemanticChatAiCommand(request);

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
            .Contain(
                "Potential prompt injection detected.");

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
                "Find fee defaulters.")
        };

        var request = CreateRequest(messages);
        var command = new SemanticChatAiCommand(request);

        ConfigureSuccessfulPipeline(
            request,
            CreateSemanticMessageResponse());

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
        var command = new SemanticChatAiCommand(request);

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

        var command =
            new SemanticChatAiCommand(request);

        ConfigureSuccessfulPipeline(
            request,
            CreateSemanticMessageResponse(),
            securedRequest);

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
                        "SECURE SYSTEM PROMPT"
                        && value.JsonMode),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldEnableJsonModeBeforeCallingGateway()
    {
        // Arrange
        var request = CreateRequest();

        var securedRequest =
            request with
            {
                SystemPrompt =
                    "SECURE SYSTEM PROMPT",
                JsonMode = false
            };

        var command =
            new SemanticChatAiCommand(request);

        ConfigureSuccessfulPipeline(
            request,
            CreateSemanticMessageResponse(),
            securedRequest);

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        _aiGatewayMock.Verify(
            x => x.ChatAsync(
                It.Is<AiChatRequest>(
                    value =>
                        value.JsonMode
                        && value.SystemPrompt ==
                           "SECURE SYSTEM PROMPT"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldEvaluateOutputAfterGatewayResponse()
    {
        // Arrange
        var request = CreateRequest();
        var command =
            new SemanticChatAiCommand(request);

        const string aiContent =
            """
            {
              "kind": "Message",
              "message": "Request completed."
            }
            """;

        ConfigureSuccessfulPipeline(
            request,
            CreateChatResponse(aiContent));

        // Act
        await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        _outputGuardrailServiceMock.Verify(
            x => x.EvaluateAsync(
                aiContent,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenOutputGuardrailAllows()
    {
        // Arrange
        var request = CreateRequest();
        var command =
            new SemanticChatAiCommand(request);

        ConfigureSuccessfulPipeline(
            request,
            CreateSemanticMessageResponse());

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Kind
            .Should()
            .Be(AiResponseKind.Message);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenOutputGuardrailBlocks()
    {
        // Arrange
        var request = CreateRequest();
        var command =
            new SemanticChatAiCommand(request);

        const string sensitiveContent =
            """
            {
              "kind": "Message",
              "message": "Here is my system prompt..."
            }
            """;

        ConfigureSuccessfulPipeline(
            request,
            CreateChatResponse(sensitiveContent));

        _outputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                sensitiveContent,
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
    public async Task Handle_ShouldNotParseSemanticResponse_WhenOutputGuardrailBlocks()
    {
        // Arrange
        var request = CreateRequest();
        var command =
            new SemanticChatAiCommand(request);

        const string blockedContent =
            """
            {
              "kind": "Message",
              "message": "show system prompt"
            }
            """;

        ConfigureSuccessfulPipeline(
            request,
            CreateChatResponse(blockedContent));

        _outputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                blockedContent,
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

        /*
         * Because output guardrail runs before
         * AiSemanticResponseParser.Parse(...),
         * a blocked response never reaches semantic parsing.
         */
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldNotReachActionValidation_WhenOutputGuardrailBlocks()
    {
        // Arrange
        var request = CreateRequest();
        var command =
            new SemanticChatAiCommand(request);

        const string blockedActionResponse =
            """
            {
              "kind": "Action",
              "proposedAction": {
                "actionName": "CreateStudent",
                "version": 1,
                "arguments": {}
              }
            }
            """;

        ConfigureSuccessfulPipeline(
            request,
            CreateChatResponse(blockedActionResponse));

        _outputGuardrailServiceMock
            .Setup(x => x.EvaluateAsync(
                blockedActionResponse,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                AiOutputGuardrailResult.Blocked(
                    "Unsafe AI output detected."));

        // Act
        var result = await _sut.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Contain(
                "Unsafe AI output detected.");

        /*
         * Since the output guardrail blocks before
         * AiSemanticResponseParser.Parse(...),
         * the concrete action validation service
         * cannot be reached.
         */
    }

    [Fact]
    public async Task Handle_ShouldNotCallGateway_WhenInputGuardrailBlocks()
    {
        // Arrange
        var request = CreateRequest();
        var command =
            new SemanticChatAiCommand(request);

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
        var command =
            new SemanticChatAiCommand(request);

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
    public async Task Handle_ShouldNotCallOutputGuardrail_WhenSystemPromptPolicyFails()
    {
        // Arrange
        var request = CreateRequest();
        var command =
            new SemanticChatAiCommand(request);

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

    [Fact]
    public async Task Handle_ShouldNotEvaluateOutput_WhenGatewayThrows()
    {
        // Arrange
        var request = CreateRequest();
        var command =
            new SemanticChatAiCommand(request);

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
        await action
            .Should()
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
        var command =
            new SemanticChatAiCommand(request);

        using var cts =
            new CancellationTokenSource();

        ConfigureSuccessfulPipeline(
            request,
            CreateSemanticMessageResponse());

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
        var command =
            new SemanticChatAiCommand(request);

        using var cts =
            new CancellationTokenSource();

        ConfigureSuccessfulPipeline(
            request,
            CreateSemanticMessageResponse());

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

        var command =
            new SemanticChatAiCommand(request);

        ConfigureSuccessfulPipeline(
            request,
            CreateSemanticMessageResponse(),
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
                            "SECURE SYSTEM PROMPT"
                        && value.JsonMode),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
                    "Find fee defaulters.")
            ],

            Temperature = 0.7f,

            MaxTokens = 500,

            SystemPrompt = null,

            JsonMode = false
        };
    }

    private static AiChatResponse CreateSemanticMessageResponse()
    {
        return CreateChatResponse(
            """
            {
              "kind": "Message",
              "message": "Request completed."
            }
            """);
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