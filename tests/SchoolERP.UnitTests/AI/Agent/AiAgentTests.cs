using System.Text.Json;
using FluentAssertions;
using Moq;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.Agent;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.UnitTests.AI.Agent;

public sealed class AiAgentTests
{
    private readonly Mock<IAiGateway> _aiGateway = new();
    private readonly Mock<IAiToolExecutor> _toolExecutor = new();
    private readonly Mock<IAiToolRegistry> _registry = new();
    private readonly Mock<IAiToolSchemaValidator> _schemaValidator = new();

    private AiActionValidationService CreateValidationService()
    {
        return new AiActionValidationService(
            _registry.Object,
            _schemaValidator.Object);
    }

    private AiAgent CreateAgent()
    {
        return new AiAgent(
            _aiGateway.Object,
            CreateValidationService(),
            _toolExecutor.Object);
    }

    private static AiAgentRequest CreateRequest(
        int maxSteps = 5)
    {
        return new AiAgentRequest
        {
            Model = "test-model",
            Messages =
            [
                new AiMessage(
                    AiMessageRole.User,
                    "Hello")
            ],
            MaxSteps = maxSteps
        };
    }

    private static AiToolExecutionResult CreateToolResult()
    {
        return new AiToolExecutionResult
        {
            ToolName = "GetStudentByAdmissionNumber",
            Output = new
            {
                Id = 1,
                Name = "Rahul"
            },
            Message = "Student found."
        };
    }

    [Fact]
    public async Task RunAsync_WhenAiReturnsMessage_ReturnsCompleted()
    {
        _aiGateway
            .Setup(x => x.ChatAsync(
                It.IsAny<AiChatRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AiChatResponse
                {
                    Content = """
                    {
                      "kind": "Message",
                      "message": "Hello!"
                    }
                    """,
                    Usage = null
                });

        var agent = CreateAgent();

        var result = await agent.RunAsync(
            CreateRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value!.State
            .Should()
            .Be(AiAgentState.Completed);

        result.Value.Message
            .Should()
            .Be("Hello!");

        result.Value.Steps
            .Should()
            .ContainSingle();

        result.Value.Steps[0].Kind
            .Should()
            .Be(AiAgentStepKind.Message);

        _toolExecutor.Verify(
            x => x.ExecuteAsync(
                It.IsAny<ToolDefinition>(),
                It.IsAny<AiActionProposal>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_WhenActionIsValid_ExecutesToolAndReplans()
    {
        var tool = new ToolDefinition
        {
            Name = "GetStudentByAdmissionNumber",
            Description = "Gets student.",
            Version = 1,
            InputSchema = new System.Text.Json.Nodes.JsonObject
            {
                ["type"] = "object"
            }
        };

        _registry
            .Setup(x => x.Get(
                "GetStudentByAdmissionNumber",
                1))
            .Returns(tool);

        _schemaValidator
            .Setup(x => x.Validate(
                tool,
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>()))
            .Returns(
                AiToolSchemaValidationResult.Success());

        _aiGateway
            .SetupSequence(x => x.ChatAsync(
                It.IsAny<AiChatRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AiChatResponse
                {
                    Content = """
                    {
                      "kind": "Action",
                      "message": "I will find the student.",
                      "proposedAction": {
                        "actionName": "GetStudentByAdmissionNumber",
                        "version": 1,
                        "arguments": {
                          "admissionNumber": "ADM001"
                        }
                      }
                    }
                    """,
                    Usage = null
                })
            .ReturnsAsync(
                new AiChatResponse
                {
                    Content = """
                    {
                      "kind": "Message",
                      "message": "Student Rahul was found."
                    }
                    """,
                    Usage = null
                });

        _toolExecutor
            .Setup(x => x.ExecuteAsync(
                tool,
                It.IsAny<AiActionProposal>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Success(
                    CreateToolResult()));

        var agent = CreateAgent();

        var result = await agent.RunAsync(
            CreateRequest());

        result.IsSuccess.Should().BeTrue();

        result.Value!.State
            .Should()
            .Be(AiAgentState.Completed);

        result.Value.Message
            .Should()
            .Be("Student Rahul was found.");

        result.Value.Steps
            .Should()
            .HaveCount(3);

        result.Value.Steps[0].Kind
            .Should()
            .Be(AiAgentStepKind.ActionProposal);

        result.Value.Steps[1].Kind
            .Should()
            .Be(AiAgentStepKind.ToolResult);

        result.Value.Steps[2].Kind
            .Should()
            .Be(AiAgentStepKind.Message);

        _toolExecutor.Verify(
            x => x.ExecuteAsync(
                tool,
                It.IsAny<AiActionProposal>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _aiGateway.Verify(
            x => x.ChatAsync(
                It.IsAny<AiChatRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _aiGateway.Verify(
            x => x.ChatAsync(
                It.Is<AiChatRequest>(request =>
                    request.Messages.Any(message =>
                        message.Role == AiMessageRole.Tool)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenActionIsInvalid_ReturnsFailure()
    {
        _registry
            .Setup(x => x.Get(
                "UnknownTool",
                1))
            .Returns((ToolDefinition?)null);

        var agent = CreateAgent();

        _aiGateway
            .Setup(x => x.ChatAsync(
                It.IsAny<AiChatRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AiChatResponse
                {
                    Content = """
                    {
                      "kind": "Action",
                      "message": "Doing it.",
                      "proposedAction": {
                        "actionName": "UnknownTool",
                        "version": 1,
                        "arguments": {}
                      }
                    }
                    """,
                    Usage = null
                });

        var result = await agent.RunAsync(
            CreateRequest());

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Validation");

        _toolExecutor.Verify(
            x => x.ExecuteAsync(
                It.IsAny<ToolDefinition>(),
                It.IsAny<AiActionProposal>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_WhenMaxStepsReached_ReturnsFailure()
    {
        var tool = new ToolDefinition
        {
            Name = "GetStudentByAdmissionNumber",
            Description = "Gets student.",
            Version = 1,
            InputSchema = new System.Text.Json.Nodes.JsonObject
            {
                ["type"] = "object"
            }
        };

        _registry
            .Setup(x => x.Get(
                "GetStudentByAdmissionNumber",
                1))
            .Returns(tool);

        _schemaValidator
            .Setup(x => x.Validate(
                tool,
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>()))
            .Returns(
                AiToolSchemaValidationResult.Success());

        _aiGateway
            .Setup(x => x.ChatAsync(
                It.IsAny<AiChatRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AiChatResponse
                {
                    Content = """
                    {
                      "kind": "Action",
                      "message": "I need another step.",
                      "proposedAction": {
                        "actionName": "GetStudentByAdmissionNumber",
                        "version": 1,
                        "arguments": {}
                      }
                    }
                    """,
                    Usage = null
                });

        _toolExecutor
            .Setup(x => x.ExecuteAsync(
                tool,
                It.IsAny<AiActionProposal>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Success(
                    CreateToolResult()));

        var agent = CreateAgent();

        var result = await agent.RunAsync(
            CreateRequest(maxSteps: 2));

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Validation");

        result.Error.Message
            .Should()
            .Contain("maximum step limit");

        _aiGateway.Verify(
            x => x.ChatAsync(
                It.IsAny<AiChatRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _toolExecutor.Verify(
            x => x.ExecuteAsync(
                tool,
                It.IsAny<AiActionProposal>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}