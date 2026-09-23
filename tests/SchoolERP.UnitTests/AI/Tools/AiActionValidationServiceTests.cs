using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.UnitTests.AI.Tools;

public sealed class AiActionValidationServiceTests
{
    [Fact]
    public void Validate_Should_Succeed_For_Registered_Tool_With_Valid_Arguments()
    {
        var tool =
            CreateStudentLookupTool();

        var registry =
            new FakeAiToolRegistry(tool);

        var schemaValidator =
            new FakeAiToolSchemaValidator(
                AiToolSchemaValidationResult.Success());

        var service =
            new AiActionValidationService(
                registry,
                schemaValidator);

        var proposal =
            CreateValidProposal();

        var result =
            service.Validate(proposal);

        result.IsValid
            .Should()
            .BeTrue();

        result.Tool
            .Should()
            .NotBeNull();

        result.Tool!.Name
            .Should()
            .Be("GetStudentByAdmissionNumber");
    }

    [Fact]
    public void Validate_Should_Fail_When_Tool_Is_Not_Registered()
    {
        var registry =
            new FakeAiToolRegistry();

        var schemaValidator =
            new FakeAiToolSchemaValidator(
                AiToolSchemaValidationResult.Success());

        var service =
            new AiActionValidationService(
                registry,
                schemaValidator);

        var proposal =
            CreateValidProposal();

        var result =
            service.Validate(proposal);

        result.IsValid
            .Should()
            .BeFalse();

        result.Tool
            .Should()
            .BeNull();

        result.Errors
            .Should()
            .ContainSingle();

        result.Errors
            .Single()
            .Should()
            .Contain("is not registered");
    }

    [Fact]
    public void Validate_Should_Fail_When_Schema_Is_Invalid()
    {
        var tool =
            CreateStudentLookupTool();

        var registry =
            new FakeAiToolRegistry(tool);

        var schemaValidator =
            new FakeAiToolSchemaValidator(
                AiToolSchemaValidationResult.Failure(
                [
                    "admissionNumber must be a string."
                ]));

        var service =
            new AiActionValidationService(
                registry,
                schemaValidator);

        var proposal =
            CreateValidProposal();

        var result =
            service.Validate(proposal);

        result.IsValid
            .Should()
            .BeFalse();

        result.Tool
            .Should()
            .BeNull();

        result.Errors
            .Should()
            .ContainSingle();

        result.Errors
            .Single()
            .Should()
            .Be(
                "admissionNumber must be a string.");
    }

    [Fact]
    public void Validate_Should_Not_Call_Schema_Validator_When_Tool_Is_Unknown()
    {
        var registry =
            new FakeAiToolRegistry();

        var schemaValidator =
            new TrackingFakeAiToolSchemaValidator();

        var service =
            new AiActionValidationService(
                registry,
                schemaValidator);

        var proposal =
            CreateValidProposal();

        service.Validate(proposal);

        schemaValidator.WasCalled
            .Should()
            .BeFalse();
    }

    [Fact]
    public void Validate_Should_Reject_Invalid_Action_Name()
    {
        var registry =
            new FakeAiToolRegistry();

        var schemaValidator =
            new FakeAiToolSchemaValidator(
                AiToolSchemaValidationResult.Success());

        var service =
            new AiActionValidationService(
                registry,
                schemaValidator);

        var proposal =
            new AiActionProposal
            {
                ActionName = string.Empty,
                Version = 1
            };

        var result =
            service.Validate(proposal);

        result.IsValid
            .Should()
            .BeFalse();

        result.Errors
            .Should()
            .Contain(
                "AI action name is required.");
    }

    [Fact]
    public void Validate_Should_Reject_Invalid_Version()
    {
        var registry =
            new FakeAiToolRegistry();

        var schemaValidator =
            new FakeAiToolSchemaValidator(
                AiToolSchemaValidationResult.Success());

        var service =
            new AiActionValidationService(
                registry,
                schemaValidator);

        var proposal =
            new AiActionProposal
            {
                ActionName =
                    "GetStudentByAdmissionNumber",
                Version = 0
            };

        var result =
            service.Validate(proposal);

        result.IsValid
            .Should()
            .BeFalse();

        result.Errors
            .Should()
            .Contain(
                "AI action version must be greater than zero.");
    }

    private static AiActionProposal CreateValidProposal()
    {
        return new AiActionProposal
        {
            ActionName =
                "GetStudentByAdmissionNumber",

            Version = 1,

            Arguments =
                new Dictionary<string, JsonElement>
                {
                    ["admissionNumber"] =
                        JsonSerializer.SerializeToElement(
                            "ADM-1001")
                }
        };
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

            InputSchema =
                new JsonObject
                {
                    ["type"] = "object",

                    ["properties"] =
                        new JsonObject
                        {
                            ["admissionNumber"] =
                                new JsonObject
                                {
                                    ["type"] = "string"
                                }
                        },

                    ["required"] =
                        new JsonArray
                        {
                            "admissionNumber"
                        },

                    ["additionalProperties"] = false
                }
        };
    }

    private sealed class FakeAiToolRegistry
        : IAiToolRegistry
    {
        private readonly ToolDefinition? _tool;

        public FakeAiToolRegistry(
            ToolDefinition? tool = null)
        {
            _tool = tool;
        }

        public void Register(
            ToolDefinition tool)
        {
            throw new NotSupportedException();
        }

        public ToolDefinition? Get(
            string name,
            int version)
        {
            if (_tool is null)
            {
                return null;
            }

            return string.Equals(
                       _tool.Name,
                       name,
                       StringComparison.OrdinalIgnoreCase)
                   && _tool.Version == version
                ? _tool
                : null;
        }

        public IReadOnlyCollection<ToolDefinition>
            GetAll()
        {
            return _tool is null
                ? []
                : [_tool];
        }
    }

    private sealed class FakeAiToolSchemaValidator
        : IAiToolSchemaValidator
    {
        private readonly AiToolSchemaValidationResult _result;

        public FakeAiToolSchemaValidator(
            AiToolSchemaValidationResult result)
        {
            _result = result;
        }

        public AiToolSchemaValidationResult Validate(
            ToolDefinition tool,
            IReadOnlyDictionary<string, JsonElement> arguments)
        {
            return _result;
        }
    }

    private sealed class TrackingFakeAiToolSchemaValidator
        : IAiToolSchemaValidator
    {
        public bool WasCalled { get; private set; }

        public AiToolSchemaValidationResult Validate(
            ToolDefinition tool,
            IReadOnlyDictionary<string, JsonElement> arguments)
        {
            WasCalled = true;

            return AiToolSchemaValidationResult.Success();
        }
    }

}