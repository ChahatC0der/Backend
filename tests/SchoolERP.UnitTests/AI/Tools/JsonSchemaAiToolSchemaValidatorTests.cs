using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Infrastructure.Services.AI.Tools;

namespace SchoolERP.UnitTests.AI.Tools;

public sealed class JsonSchemaAiToolSchemaValidatorTests
{
    [Fact]
    public void Validate_Should_Return_Success_For_Valid_Arguments()
    {
        var validator =
            new JsonSchemaAiToolSchemaValidator();

        var tool = CreateStudentLookupTool();

        var arguments =
            new Dictionary<string, JsonElement>
            {
                ["admissionNumber"] =
                    JsonSerializer.SerializeToElement(
                        "ADM-1001")
            };

        var result =
            validator.Validate(
                tool,
                arguments);

        result.IsValid
            .Should()
            .BeTrue();

        result.Errors
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Validate_Should_Reject_Wrong_Argument_Type()
    {
        var validator =
            new JsonSchemaAiToolSchemaValidator();

        var tool = CreateStudentLookupTool();

        var arguments =
            new Dictionary<string, JsonElement>
            {
                ["admissionNumber"] =
                    JsonSerializer.SerializeToElement(
                        1001)
            };

        var result =
            validator.Validate(
                tool,
                arguments);

        result.IsValid
            .Should()
            .BeFalse();

        result.Errors
            .Should()
            .NotBeEmpty();
    }

    [Fact]
    public void Validate_Should_Reject_Missing_Required_Argument()
    {
        var validator =
            new JsonSchemaAiToolSchemaValidator();

        var tool = CreateStudentLookupTool();

        var arguments =
            new Dictionary<string, JsonElement>();

        var result =
            validator.Validate(
                tool,
                arguments);

        result.IsValid
            .Should()
            .BeFalse();

        result.Errors
            .Should()
            .NotBeEmpty();
    }

    [Fact]
    public void Validate_Should_Reject_Unknown_Argument()
    {
        var validator =
            new JsonSchemaAiToolSchemaValidator();

        var tool = CreateStudentLookupTool();

        var arguments =
            new Dictionary<string, JsonElement>
            {
                ["admissionNumber"] =
                    JsonSerializer.SerializeToElement(
                        "ADM-1001"),

                ["unknownField"] =
                    JsonSerializer.SerializeToElement(
                        true)
            };

        var result =
            validator.Validate(
                tool,
                arguments);

        result.IsValid
            .Should()
            .BeFalse();

        result.Errors
            .Should()
            .NotBeEmpty();
    }

    [Fact]
    public void Validate_Should_Reject_Invalid_Tool_Schema()
    {
        var validator =
            new JsonSchemaAiToolSchemaValidator();

        var tool = new ToolDefinition
        {
            Name = "InvalidTool",

            Description = "Invalid schema test.",

            Version = 1,

            InputSchema = JsonNode.Parse(
                """
                {
                  "type": "not-a-real-json-type"
                }
                """)!.AsObject()
        };

        var result =
            validator.Validate(
                tool,
                new Dictionary<string, JsonElement>());

        result.IsValid
            .Should()
            .BeFalse();

        result.Errors
            .Should()
            .ContainSingle();

        result.Errors
            .Single()
            .Should()
            .Contain("invalid input schema");
    }

    private static ToolDefinition CreateStudentLookupTool()
    {
        return new ToolDefinition
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
            }
        };
    }
}