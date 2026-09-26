using Moq;
using SchoolERP.Application.Features.AI.Agent;
using SchoolERP.Application.Features.AI.Confirmation;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Streaming;
using SchoolERP.Application.Features.AI.Tools;
using System.Text.Json;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Streaming;

public sealed class AiAgentStreamServiceTests
{
    private readonly Mock<IAiAgent> _aiAgentMock;
    private readonly AiAgentStreamService _service;

    public AiAgentStreamServiceTests()
    {
        _aiAgentMock = new Mock<IAiAgent>();

        _service = new AiAgentStreamService(
            _aiAgentMock.Object);
    }

    [Fact]
    public async Task StreamAsync_Should_Map_All_Agent_Events()
    {
        var request = CreateRequest();

        _aiAgentMock
            .Setup(x => x.RunStreamAsync(
                It.IsAny<AiAgentRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(CreateAgentEvents());

        var events = await CollectAsync(
            _service.StreamAsync(request));

        Assert.Equal(6, events.Count);

        Assert.Equal(
            AiStreamEventType.ToolProposed,
            events[0].Type);

        Assert.Equal(
            AiStreamEventType.ToolExecutionStarted,
            events[1].Type);

        Assert.Equal(
            AiStreamEventType.ConfirmationRequired,
            events[2].Type);

        Assert.Equal(
            AiStreamEventType.ToolExecutionCompleted,
            events[3].Type);

        Assert.Equal(
            AiStreamEventType.MessageCompleted,
            events[4].Type);

        Assert.Equal(
            AiStreamEventType.Error,
            events[5].Type);
    }

    [Fact]
    public async Task StreamAsync_Should_Map_ToolProposed_Data()
    {
        var request = CreateRequest();

        var action = new AiActionProposal
        {
            ActionName = "GetStudentByAdmissionNumber",
            Version = 1,
            Arguments = new Dictionary<string, JsonElement>
            {
                ["admissionNumber"] =
                    JsonSerializer.SerializeToElement("ADM-1001")
            }
        };

        _aiAgentMock
            .Setup(x => x.RunStreamAsync(
                It.IsAny<AiAgentRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(CreateSingleEvent(
                new AiAgentEvent
                {
                    Type = AiAgentEventType.ToolProposed,
                    Message = "Student lookup proposed.",
                    Data = action
                }));

        var events = await CollectAsync(
            _service.StreamAsync(request));

        var streamEvent = Assert.Single(events);

        Assert.Equal(
            AiStreamEventType.ToolProposed,
            streamEvent.Type);

        Assert.Equal(
            "Student lookup proposed.",
            streamEvent.Content);

        Assert.NotNull(streamEvent.Data);

        var data = streamEvent.Data!.Value;

        Assert.Equal(
            "GetStudentByAdmissionNumber",
            data.GetProperty("actionName").GetString());

        Assert.Equal(
            1,
            data.GetProperty("version").GetInt32());

        Assert.Equal(
            "ADM-1001",
            data.GetProperty("arguments")
                .GetProperty("admissionNumber")
                .GetString());
    }

    [Fact]
    public async Task StreamAsync_Should_Map_ToolExecutionStarted_Data()
    {
        var request = CreateRequest();

        _aiAgentMock
            .Setup(x => x.RunStreamAsync(
                It.IsAny<AiAgentRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(CreateSingleEvent(
                new AiAgentEvent
                {
                    Type = AiAgentEventType.ToolExecutionStarted,
                    Data = new
                    {
                        Tool = "GetStudentByAdmissionNumber",
                        Version = 1
                    }
                }));

        var events = await CollectAsync(
            _service.StreamAsync(request));

        var streamEvent = Assert.Single(events);

        Assert.Equal(
            AiStreamEventType.ToolExecutionStarted,
            streamEvent.Type);

        Assert.NotNull(streamEvent.Data);

        var data = streamEvent.Data!.Value;

        Assert.Equal(
            "GetStudentByAdmissionNumber",
            data.GetProperty("toolName").GetString());

        Assert.Equal(
            1,
            data.GetProperty("version").GetInt32());
    }

    [Fact]
    public async Task StreamAsync_Should_Map_ConfirmationRequired_Data()
    {
        var request = CreateRequest();

        var requirement = new AiConfirmationRequirement
        {
            ConfirmationId = Guid.NewGuid(),
            ConfirmationToken = "test-confirmation-token",
            ToolName = "DeleteStudent",
            Version = 1,
            RiskLevel = AiRiskLevel.High,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(5),
            Reasons =
            [
                "This action modifies student data."
            ]
        };

        var executionResult =
            AiToolExecutionResult.ConfirmationRequired(
                requirement);

        _aiAgentMock
            .Setup(x => x.RunStreamAsync(
                It.IsAny<AiAgentRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(CreateSingleEvent(
                new AiAgentEvent
                {
                    Type = AiAgentEventType.ConfirmationRequired,
                    Data = executionResult,
                    Message = executionResult.Message
                }));

        var events = await CollectAsync(
            _service.StreamAsync(request));

        var streamEvent = Assert.Single(events);

        Assert.Equal(
            AiStreamEventType.ConfirmationRequired,
            streamEvent.Type);

        Assert.Equal(
            executionResult.Message,
            streamEvent.Content);

        Assert.NotNull(streamEvent.Data);

        var data = streamEvent.Data!.Value;

        Assert.Equal(
            "DeleteStudent",
            data.GetProperty("toolName").GetString());

        Assert.Equal(
            executionResult.Message,
            data.GetProperty("message").GetString());

        var confirmation =
            data.GetProperty("confirmation");

        Assert.Equal(
            "test-confirmation-token",
            confirmation
                .GetProperty("confirmationToken")
                .GetString());

        Assert.Equal(
            "DeleteStudent",
            confirmation
                .GetProperty("toolName")
                .GetString());

        Assert.Equal(
            1,
            confirmation
                .GetProperty("version")
                .GetInt32());
    }

    [Fact]
    public async Task StreamAsync_Should_Map_ToolExecutionCompleted_Data()
    {
        var request = CreateRequest();

        _aiAgentMock
            .Setup(x => x.RunStreamAsync(
                It.IsAny<AiAgentRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(CreateSingleEvent(
                new AiAgentEvent
                {
                    Type = AiAgentEventType.ToolExecutionCompleted,
                    Data = new
                    {
                        Tool = "GetStudentByAdmissionNumber",
                        Message = "Student found."
                    }
                }));

        var events = await CollectAsync(
            _service.StreamAsync(request));

        var streamEvent = Assert.Single(events);

        Assert.Equal(
            AiStreamEventType.ToolExecutionCompleted,
            streamEvent.Type);

        Assert.NotNull(streamEvent.Data);

        var data = streamEvent.Data!.Value;

        Assert.Equal(
            "GetStudentByAdmissionNumber",
            data.GetProperty("toolName").GetString());

        Assert.Equal(
            "Student found.",
            data.GetProperty("message").GetString());
    }

    [Fact]
    public async Task StreamAsync_Should_Map_MessageCompleted_Data()
    {
        var request = CreateRequest();

        _aiAgentMock
            .Setup(x => x.RunStreamAsync(
                It.IsAny<AiAgentRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(CreateSingleEvent(
                new AiAgentEvent
                {
                    Type = AiAgentEventType.MessageCompleted,
                    Message = "Student found.",
                    Data = new
                    {
                        State = AiAgentState.Completed
                    }
                }));

        var events = await CollectAsync(
            _service.StreamAsync(request));

        var streamEvent = Assert.Single(events);

        Assert.Equal(
            AiStreamEventType.MessageCompleted,
            streamEvent.Type);

        Assert.Equal(
            "Student found.",
            streamEvent.Content);

        Assert.NotNull(streamEvent.Data);

        var data = streamEvent.Data!.Value;

        Assert.Equal(
            "Completed",
            data.GetProperty("state").GetString());
    }

    [Fact]
    public async Task StreamAsync_Should_Map_Error_Data()
    {
        var request = CreateRequest();

        _aiAgentMock
            .Setup(x => x.RunStreamAsync(
                It.IsAny<AiAgentRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(CreateSingleEvent(
                new AiAgentEvent
                {
                    Type = AiAgentEventType.Error,
                    Message = "Tool execution failed.",
                    Data = new
                    {
                        Code = "TOOL_FAILED"
                    }
                }));

        var events = await CollectAsync(
            _service.StreamAsync(request));

        var streamEvent = Assert.Single(events);

        Assert.Equal(
            AiStreamEventType.Error,
            streamEvent.Type);

        Assert.Equal(
            "Tool execution failed.",
            streamEvent.Content);

        Assert.NotNull(streamEvent.Data);

        var data = streamEvent.Data!.Value;

        Assert.Equal(
            "Tool execution failed.",
            data.GetProperty("message").GetString());

        Assert.Equal(
            "TOOL_FAILED",
            data.GetProperty("details")
                .GetProperty("code")
                .GetString());
    }

    [Fact]
    public async Task StreamAsync_Should_Preserve_Timestamp()
    {
        var request = CreateRequest();

        var timestamp =
            DateTimeOffset.UtcNow.AddMinutes(-1);

        _aiAgentMock
            .Setup(x => x.RunStreamAsync(
                It.IsAny<AiAgentRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(CreateSingleEvent(
                new AiAgentEvent
                {
                    Type = AiAgentEventType.MessageCompleted,
                    Message = "Done.",
                    TimestampUtc = timestamp
                }));

        var events = await CollectAsync(
            _service.StreamAsync(request));

        var streamEvent = Assert.Single(events);

        Assert.Equal(
            timestamp,
            streamEvent.TimestampUtc);
    }

    [Fact]
    public async Task StreamAsync_Should_Forward_Request_And_CancellationToken()
    {
        var request = CreateRequest();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var cancellationToken =
            cancellationTokenSource.Token;

        _aiAgentMock
            .Setup(x => x.RunStreamAsync(
                It.IsAny<AiAgentRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(CreateEmptyEvents());

        await CollectAsync(
            _service.StreamAsync(
                request,
                cancellationToken));

        _aiAgentMock.Verify(
            x => x.RunStreamAsync(
                request,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task StreamAsync_Should_Throw_When_Request_Is_Null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () =>
            {
                await CollectAsync(
                    _service.StreamAsync(null!));
            });
    }

    [Fact]
    public async Task StreamAsync_Should_Throw_For_Unsupported_Event()
    {
        var request = CreateRequest();

        _aiAgentMock
            .Setup(x => x.RunStreamAsync(
                It.IsAny<AiAgentRequest>(),
                It.IsAny<CancellationToken>()))
            .Returns(CreateSingleEvent(
                new AiAgentEvent
                {
                    Type = (AiAgentEventType)999
                }));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () =>
            {
                await CollectAsync(
                    _service.StreamAsync(request));
            });
    }

    private static AiAgentRequest CreateRequest()
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
            MaxSteps = 5
        };
    }

    private static async IAsyncEnumerable<AiAgentEvent>
        CreateAgentEvents()
    {
        yield return new AiAgentEvent
        {
            Type = AiAgentEventType.ToolProposed,
            Data = new AiActionProposal
            {
                ActionName = "GetStudentByAdmissionNumber",
                Version = 1
            }
        };

        yield return new AiAgentEvent
        {
            Type = AiAgentEventType.ToolExecutionStarted,
            Data = new
            {
                Tool = "GetStudentByAdmissionNumber",
                Version = 1
            }
        };

        yield return new AiAgentEvent
        {
            Type = AiAgentEventType.ConfirmationRequired,
            Data = AiToolExecutionResult.ConfirmationRequired(
                new AiConfirmationRequirement
                {
                    ConfirmationId = Guid.NewGuid(),
                    ConfirmationToken = "token",
                    ToolName = "DeleteStudent",
                    Version = 1,
                    RiskLevel = AiRiskLevel.High,
                    ExpiresAtUtc =
                        DateTimeOffset.UtcNow.AddMinutes(5),
                    Reasons =
                    [
                        "Confirmation required."
                    ]
                })
        };

        yield return new AiAgentEvent
        {
            Type = AiAgentEventType.ToolExecutionCompleted,
            Data = new
            {
                Tool = "GetStudentByAdmissionNumber",
                Message = "Student found."
            }
        };

        yield return new AiAgentEvent
        {
            Type = AiAgentEventType.MessageCompleted,
            Message = "Student found.",
            Data = new
            {
                State = AiAgentState.Completed
            }
        };

        yield return new AiAgentEvent
        {
            Type = AiAgentEventType.Error,
            Message = "Error."
        };

        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<AiAgentEvent>
        CreateSingleEvent(
            AiAgentEvent agentEvent)
    {
        yield return agentEvent;

        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<AiAgentEvent>
        CreateEmptyEvents()
    {
        await Task.CompletedTask;
        yield break;
    }

    private static async Task<List<AiStreamEvent>>
        CollectAsync(
            IAsyncEnumerable<AiStreamEvent> stream)
    {
        var events = new List<AiStreamEvent>();

        await foreach (var streamEvent in stream)
        {
            events.Add(streamEvent);
        }

        return events;
    }
}