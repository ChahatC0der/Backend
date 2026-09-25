using FluentAssertions;
using SchoolERP.Application.Features.AI.Confirmation;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Tools;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.DTOs;

public sealed class AiToolExecutionResultTests
{
    [Fact]
    public void Executed_should_create_executed_result()
    {
        var result =
            AiToolExecutionResult.Executed(
                toolName: "GetStudentByAdmissionNumber",
                output: new
                {
                    AdmissionNumber = "ADM-001",
                    Name = "Rahul"
                },
                message: "Student found.");

        result.ToolName
            .Should()
            .Be("GetStudentByAdmissionNumber");

        result.Status
            .Should()
            .Be(AiToolExecutionStatus.Executed);

        result.RequiresConfirmation
            .Should()
            .BeFalse();

        result.Output
            .Should()
            .NotBeNull();

        result.Message
            .Should()
            .Be("Student found.");

        result.Confirmation
            .Should()
            .BeNull();
    }

    [Fact]
    public void ConfirmationRequired_should_create_confirmation_result()
    {
        var requirement =
            new AiConfirmationRequirement
            {
                ConfirmationId =
                    Guid.NewGuid(),

                ConfirmationToken =
                    "protected-confirmation-token",

                ToolName =
                    "DeleteStudent",

                Version =
                    1,

                RiskLevel =
                    AiRiskLevel.High,

                ExpiresAtUtc =
                    DateTimeOffset.UtcNow.AddMinutes(5),

                Reasons =
                [
                    "High-risk operation."
                ]
            };

        var result =
            AiToolExecutionResult.ConfirmationRequired(
                requirement);

        result.ToolName
            .Should()
            .Be("DeleteStudent");

        result.Status
            .Should()
            .Be(AiToolExecutionStatus.ConfirmationRequired);

        result.RequiresConfirmation
            .Should()
            .BeTrue();

        result.Output
            .Should()
            .BeNull();

        result.Confirmation
            .Should()
            .BeSameAs(requirement);

        result.Message
            .Should()
            .Contain("Confirmation is required");

        result.Message
            .Should()
            .Contain("DeleteStudent");
    }

    [Fact]
    public void Default_status_should_be_executed()
    {
        var result =
            new AiToolExecutionResult
            {
                ToolName =
                    "GetStudentByAdmissionNumber",
                Output =
                    new
                    {
                        AdmissionNumber =
                            "ADM-001"
                    }
            };

        result.Status
            .Should()
            .Be(AiToolExecutionStatus.Executed);

        result.RequiresConfirmation
            .Should()
            .BeFalse();
    }
}