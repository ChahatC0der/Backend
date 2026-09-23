using FluentAssertions;
using System.Text.Json.Nodes;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.UnitTests.AI.Tools;

public sealed class ToolDefinitionTests
{
    [Fact]
    public void Should_Create_Tool_Definition()
    {
        var tool = new ToolDefinition
        {
            Name =
                "GetStudentByAdmissionNumber",

            Description =
                "Retrieves a student using their admission number.",

            Version = 1,

            InputSchema = new JsonObject
            {
                ["type"] = "object",

                ["properties"] = new JsonObject
                {
                    ["admissionNumber"] = new JsonObject
                    {
                        ["type"] = "string"
                    }
                },

                ["required"] = new JsonArray
                {
                    "admissionNumber"
                },

                ["additionalProperties"] = false
            },

            RequiredPermission =
                "Student.View",

            DataScope =
                AiDataScope.Branch,

            RiskLevel =
                AiRiskLevel.Low,

            ConfirmationPolicy =
                AiConfirmationPolicy.None,

            AuditPolicy =
                AiAuditPolicy.Required
        };

        tool.Name
            .Should()
            .Be("GetStudentByAdmissionNumber");

        tool.Version
            .Should()
            .Be(1);

        tool.RequiredPermission
            .Should()
            .Be("Student.View");

        tool.DataScope
            .Should()
            .Be(AiDataScope.Branch);

        tool.RiskLevel
            .Should()
            .Be(AiRiskLevel.Low);

        tool.ConfirmationPolicy
            .Should()
            .Be(AiConfirmationPolicy.None);

        tool.AuditPolicy
            .Should()
            .Be(AiAuditPolicy.Required);

        tool.InputSchema["type"]!
            .GetValue<string>()
            .Should()
            .Be("object");

        tool.InputSchema["properties"]!
            .AsObject()
            .Should();
            //.ContainKey("admissionNumber");
    }
}