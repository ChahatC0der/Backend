using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Moq;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.Commands.SemanticChat;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.UnitTests.AI;

public sealed class SemanticChatAiCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Failure_When_Action_Arguments_Are_Invalid()
    {
        var gateway =
            new Mock<IAiGateway>();

        gateway
            .Setup(x =>
                x.ChatAsync(
                    It.IsAny<AiChatRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AiChatResponse
                {
                    Content =
                        """
                    {
                      "kind": "Action",
                      "message": "Fetching student.",
                      "proposedAction": {
                        "actionName": "GetStudentByAdmissionNumber",
                        "version": 1,
                        "arguments": {
                          "admissionNumber": 123
                        }
                      }
                    }
                    """
                });

        var tool =
            CreateStudentLookupTool();

        var registry =
            new Mock<IAiToolRegistry>();

        registry
            .Setup(x =>
                x.Get(
                    "GetStudentByAdmissionNumber",
                    1))
            .Returns(tool);

        var schemaValidator =
            new Mock<IAiToolSchemaValidator>();

        schemaValidator
            .Setup(x =>
                x.Validate(
                    tool,
                    It.IsAny<IReadOnlyDictionary<string, JsonElement>>()))
            .Returns(
                AiToolSchemaValidationResult.Failure(
                [
                    "admissionNumber must be a string."
                ]));

        var validationService =
            new AiActionValidationService(
                registry.Object,
                schemaValidator.Object);

        var handler =
            new SemanticChatAiCommandHandler(
                gateway.Object,
                validationService);

        var result =
            await handler.Handle(
                new SemanticChatAiCommand(
                    new AiChatRequest
                    {
                        Model = "test-model",
                        Messages =
                        [
                            new AiMessage(
                            AiMessageRole.User,
                            "Show student")
                        ]
                    }),
                CancellationToken.None);

        result.IsFailure
            .Should()
            .BeTrue();

        result.Error.Code
            .Should()
            .Be("Validation");

        result.Error.Message
            .Should()
            .Contain(
                "admissionNumber must be a string.");
    }
    [Fact]
    public async Task Handle_Should_Return_Failure_When_Action_Is_Not_Registered()
    {
        var gateway =
            new Mock<IAiGateway>();

        gateway
            .Setup(x =>
                x.ChatAsync(
                    It.IsAny<AiChatRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AiChatResponse
                {
                    Content =
                        """
                    {
                      "kind": "Action",
                      "message": "Performing unknown action.",
                      "proposedAction": {
                        "actionName": "UnknownAction",
                        "version": 1,
                        "arguments": {}
                      }
                    }
                    """
                });

        var registry =
            new Mock<IAiToolRegistry>();

        registry
            .Setup(x =>
                x.Get(
                    "UnknownAction",
                    1))
            .Returns((ToolDefinition?)null);

        var schemaValidator =
            new Mock<IAiToolSchemaValidator>();

        var validationService =
            new AiActionValidationService(
                registry.Object,
                schemaValidator.Object);

        var handler =
            new SemanticChatAiCommandHandler(
                gateway.Object,
                validationService);

        var command =
            new SemanticChatAiCommand(
                new AiChatRequest
                {
                    Model = "test-model",
                    Messages =
                    [
                        new AiMessage(
                        AiMessageRole.User,
                        "Do something")
                    ]
                });

        var result =
            await handler.Handle(
                command,
                CancellationToken.None);

        result.IsFailure
            .Should()
            .BeTrue();

        result.Error.Code
            .Should()
            .Be("Validation");

        result.Error.Message
            .Should()
            .Contain("is not registered");
    }
    [Fact]
    public async Task Handle_Should_Return_Action_Response_When_Action_Is_Valid()
    {
        var gateway =
            new Mock<IAiGateway>();

        gateway
            .Setup(x =>
                x.ChatAsync(
                    It.IsAny<AiChatRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AiChatResponse
                {
                    Content =
                        """
                        {
                          "kind": "Action",
                          "message": "Fetching student details.",
                          "proposedAction": {
                            "actionName": "GetStudentByAdmissionNumber",
                            "version": 1,
                            "arguments": {
                              "admissionNumber": "ADM-1001"
                            }
                          }
                        }
                        """
                });

        var tool =
            CreateStudentLookupTool();

        var registry =
            new Mock<IAiToolRegistry>();

        registry
            .Setup(x =>
                x.Get(
                    "GetStudentByAdmissionNumber",
                    1))
            .Returns(tool);

        var schemaValidator =
            new Mock<IAiToolSchemaValidator>();

        schemaValidator
            .Setup(x =>
                x.Validate(
                    tool,
                    It.IsAny<IReadOnlyDictionary<string, JsonElement>>()))
            .Returns(
                AiToolSchemaValidationResult.Success());

        var validationService =
            new AiActionValidationService(
                registry.Object,
                schemaValidator.Object);

        var handler =
            new SemanticChatAiCommandHandler(
                gateway.Object,
                validationService);

        var result =
            await handler.Handle(
                CreateCommand(),
                CancellationToken.None);

        result.IsSuccess
            .Should()
            .BeTrue();

        result.Value.Kind
            .Should()
            .Be(AiResponseKind.Action);

        result.Value.Message
            .Should()
            .Be("Fetching student details.");

        result.Value.ProposedAction
            .Should()
            .NotBeNull();

        result.Value.ProposedAction!.ActionName
            .Should()
            .Be("GetStudentByAdmissionNumber");

        result.Value.ProposedAction.Version
            .Should()
            .Be(1);

        registry.Verify(
            x =>
                x.Get(
                    "GetStudentByAdmissionNumber",
                    1),
            Times.Once);

        schemaValidator.Verify(
            x =>
                x.Validate(
                    tool,
                    It.IsAny<IReadOnlyDictionary<string, JsonElement>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Message_When_No_Action_Is_Proposed()
    {
        var gateway =
            new Mock<IAiGateway>();

        gateway
            .Setup(x =>
                x.ChatAsync(
                    It.IsAny<AiChatRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AiChatResponse
                {
                    Content =
                        """
                        {
                          "kind": "Message",
                          "message": "Hello SchoolERP"
                        }
                        """
                });

        var registry =
            new Mock<IAiToolRegistry>();

        var schemaValidator =
            new Mock<IAiToolSchemaValidator>();

        var validationService =
            new AiActionValidationService(
                registry.Object,
                schemaValidator.Object);

        var handler =
            new SemanticChatAiCommandHandler(
                gateway.Object,
                validationService);

        var result =
            await handler.Handle(
                CreateCommand(),
                CancellationToken.None);

        result.IsSuccess
            .Should()
            .BeTrue();

        result.Value.Kind
            .Should()
            .Be(AiResponseKind.Message);

        result.Value.Message
            .Should()
            .Be("Hello SchoolERP");

        result.Value.ProposedAction
            .Should()
            .BeNull();

        registry.Verify(
            x =>
                x.Get(
                    It.IsAny<string>(),
                    It.IsAny<int>()),
            Times.Never);

        schemaValidator.Verify(
            x =>
                x.Validate(
                    It.IsAny<ToolDefinition>(),
                    It.IsAny<IReadOnlyDictionary<string, JsonElement>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Force_JsonMode()
    {
        var gateway =
            new Mock<IAiGateway>();

        gateway
            .Setup(x =>
                x.ChatAsync(
                    It.IsAny<AiChatRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AiChatResponse
                {
                    Content =
                        """
                        {
                          "kind": "Message",
                          "message": "Hello"
                        }
                        """
                });

        var registry =
            new Mock<IAiToolRegistry>();

        var schemaValidator =
            new Mock<IAiToolSchemaValidator>();

        var validationService =
            new AiActionValidationService(
                registry.Object,
                schemaValidator.Object);

        var handler =
            new SemanticChatAiCommandHandler(
                gateway.Object,
                validationService);

        await handler.Handle(
            CreateCommand(
                jsonMode: false),
            CancellationToken.None);

        gateway.Verify(
            x =>
                x.ChatAsync(
                    It.Is<AiChatRequest>(
                        request =>
                            request.JsonMode),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Pass_Original_User_Request_To_Gateway()
    {
        var gateway =
            new Mock<IAiGateway>();

        gateway
            .Setup(x =>
                x.ChatAsync(
                    It.IsAny<AiChatRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AiChatResponse
                {
                    Content =
                        """
                        {
                          "kind": "Message",
                          "message": "Hello"
                        }
                        """
                });

        var registry =
            new Mock<IAiToolRegistry>();

        var schemaValidator =
            new Mock<IAiToolSchemaValidator>();

        var validationService =
            new AiActionValidationService(
                registry.Object,
                schemaValidator.Object);

        var handler =
            new SemanticChatAiCommandHandler(
                gateway.Object,
                validationService);

        var command =
            CreateCommand();

        await handler.Handle(
            command,
            CancellationToken.None);

        gateway.Verify(
            x =>
                x.ChatAsync(
                    It.Is<AiChatRequest>(
                        request =>
                            request.Model ==
                                "test-model"
                            && request.Messages.Count == 1
                            && request.Messages[0].Content ==
                                "Show ADM-1001"
                            && request.JsonMode),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static SemanticChatAiCommand CreateCommand(
        bool jsonMode = false)
    {
        return new SemanticChatAiCommand(
            new AiChatRequest
            {
                Model = "test-model",

                Messages =
                [
                    new AiMessage(
                        AiMessageRole.User,
                        "Show ADM-1001")
                ],

                JsonMode = jsonMode
            });
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

                    ["additionalProperties"] =
                        false
                }
        };
    }
}