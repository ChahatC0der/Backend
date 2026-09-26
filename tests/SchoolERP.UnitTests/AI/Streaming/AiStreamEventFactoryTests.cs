using FluentAssertions;
using SchoolERP.Application.Features.AI.Confirmation;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Streaming;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.UnitTests.Features.AI.Streaming;

public sealed class AiStreamEventFactoryTests
{
    [Fact]
    public void ToolProposed_should_create_tool_proposed_event()
    {
        var result =
            AiStreamEventFactory.ToolProposed(
                "GetStudentByAdmissionNumber",
                1,
                new
                {
                    admissionNumber = "ADM-001"
                });

        result.Type
            .Should()
            .Be(AiStreamEventType.ToolProposed);

        result.Data
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void ConfirmationRequired_should_preserve_confirmation_contract()
    {
        var confirmation =
            new AiConfirmationRequirement
            {
                ConfirmationId = Guid.NewGuid(),
                ConfirmationToken = "token",
                ToolName = "DeactivateStudent",
                Version = 1,
                RiskLevel = AiRiskLevel.High,
                ExpiresAtUtc =
                    DateTimeOffset.UtcNow.AddMinutes(5),
                Reasons =
                [
                    "High-risk action requires confirmation."
                ]
            };

        var result =
            AiStreamEventFactory.ConfirmationRequired(
                confirmation);

        result.Type
            .Should()
            .Be(AiStreamEventType.ConfirmationRequired);

        result.Data
            .Should()
            .BeSameAs(confirmation);
    }

    [Fact]
    public void ToolExecutionStarted_should_include_tool_identity()
    {
        var result =
            AiStreamEventFactory.ToolExecutionStarted(
                "DeactivateStudent",
                1);

        result.Type
            .Should()
            .Be(AiStreamEventType.ToolExecutionStarted);

        result.Data
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void ToolExecutionCompleted_should_preserve_execution_result()
    {
        var executionResult =
            AiToolExecutionResult.Executed(
                "GetStudentByAdmissionNumber",
                new
                {
                    AdmissionNumber = "ADM-001"
                });

        var result =
            AiStreamEventFactory.ToolExecutionCompleted(
                executionResult);

        result.Type
            .Should()
            .Be(AiStreamEventType.ToolExecutionCompleted);

        result.Data
            .Should()
            .BeSameAs(executionResult);
    }

    [Fact]
    public void Error_should_create_error_event()
    {
        var result =
            AiStreamEventFactory.Error(
                "AI tool execution failed.");

        result.Type
            .Should()
            .Be(AiStreamEventType.Error);

        result.Content
            .Should()
            .Be("AI tool execution failed.");
    }
}