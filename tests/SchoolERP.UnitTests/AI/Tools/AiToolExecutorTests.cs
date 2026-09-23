using System.Text.Json;
using FluentAssertions;
using Moq;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.UnitTests.AI.Tools;

public sealed class AiToolExecutorTests
{
    private static ToolDefinition CreateTool()
    {
        return new ToolDefinition
        {
            Name = "GetStudentByAdmissionNumber",
            Description = "Gets a student by admission number.",
            Version = 1,
            InputSchema = new System.Text.Json.Nodes.JsonObject
            {
                ["type"] = "object"
            }
        };
    }

    private static AiActionProposal CreateAction(
        string actionName = "GetStudentByAdmissionNumber",
        int version = 1)
    {
        return new AiActionProposal
        {
            ActionName = actionName,
            Version = version,
            Arguments = new Dictionary<string, JsonElement>()
        };
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerExists_ExecutesHandler()
    {
        var handler = new Mock<IAiToolHandler>();

        handler.SetupGet(x => x.Name)
            .Returns("GetStudentByAdmissionNumber");

        handler.SetupGet(x => x.Version)
            .Returns(1);

        Result<AiToolExecutionResult> successResult =
            Result.Success(
                new AiToolExecutionResult
                {
                    ToolName = "GetStudentByAdmissionNumber",
                    Output = new
                    {
                        Id = 1
                    }
                });

        handler.Setup(x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        var executor = new AiToolExecutor(
            [handler.Object]);

        var tool = CreateTool();
        var action = CreateAction();

        var result = await executor.ExecuteAsync(
            tool,
            action);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ToolName
            .Should()
            .Be("GetStudentByAdmissionNumber");

        handler.Verify(
            x => x.ExecuteAsync(
                action.Arguments,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerDoesNotExist_ReturnsValidationFailure()
    {
        var executor = new AiToolExecutor(
            Array.Empty<IAiToolHandler>());

        var tool = CreateTool();
        var action = CreateAction();

        var result = await executor.ExecuteAsync(
            tool,
            action);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        result.Error.Message
            .Should()
            .Contain("No executor is registered");
    }

    [Fact]
    public async Task ExecuteAsync_WhenToolNameDoesNotMatch_ReturnsValidationFailure()
    {
        var handler = new Mock<IAiToolHandler>();

        var executor = new AiToolExecutor(
            [handler.Object]);

        var tool = CreateTool();

        var action = CreateAction(
            actionName: "CreateStudent");

        var result = await executor.ExecuteAsync(
            tool,
            action);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        result.Error.Message
            .Should()
            .Contain("action name do not match");

        handler.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVersionDoesNotMatch_ReturnsValidationFailure()
    {
        var handler = new Mock<IAiToolHandler>();

        var executor = new AiToolExecutor(
            [handler.Object]);

        var tool = CreateTool();

        var action = CreateAction(
            version: 2);

        var result = await executor.ExecuteAsync(
            tool,
            action);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        result.Error.Message
            .Should()
            .Contain("version do not match");

        handler.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerFails_PropagatesFailure()
    {
        var handler = new Mock<IAiToolHandler>();

        handler.SetupGet(x => x.Name)
            .Returns("GetStudentByAdmissionNumber");

        handler.SetupGet(x => x.Version)
            .Returns(1);

        Result<AiToolExecutionResult> failureResult =
            Result.Failure<AiToolExecutionResult>(
                Error.Validation("Student could not be found."));

        handler.Setup(x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        var executor = new AiToolExecutor(
            [handler.Object]);

        var tool = CreateTool();
        var action = CreateAction();

        var result = await executor.ExecuteAsync(
            tool,
            action);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        result.Error.Message
            .Should()
            .Be("Student could not be found.");
    }
}