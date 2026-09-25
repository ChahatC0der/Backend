using FluentAssertions;
using Moq;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.Confirmation;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Risk;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Domain.Shared.Results;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Tools;

public sealed class AiToolExecutorTests
{
    [Fact]
    public async Task Should_fail_when_tool_is_null()
    {
        var contextAccessor =
            CreateContextAccessor(true);

        var authorizationService =
            CreateAuthorizationService(contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var handler =
            CreateHandler();

        var executor =
            CreateExecutor(
                handler.Object,
                authorizationService,
                riskClassifier.Object,
                contextAccessor.Object,
                confirmationService.Object);

        var result =
            await executor.ExecuteAsync(
                null!,
                CreateAction("GetStudentByAdmissionNumber"));

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Be("Tool definition is required.");

        riskClassifier.Verify(
            x => x.Classify(It.IsAny<ToolDefinition>()),
            Times.Never);

        confirmationService.Verify(
            x => x.CreatePendingAction(
                It.IsAny<ToolDefinition>(),
                It.IsAny<AiActionProposal>(),
                It.IsAny<AiExecutionContext>(),
                It.IsAny<AiRiskAssessment>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_fail_when_action_is_null()
    {
        var contextAccessor =
            CreateContextAccessor(true);

        var authorizationService =
            CreateAuthorizationService(contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var executor =
            CreateExecutor(
                CreateHandler().Object,
                authorizationService,
                riskClassifier.Object,
                contextAccessor.Object,
                confirmationService.Object);

        var result =
            await executor.ExecuteAsync(
                CreateTool(),
                null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Be("Action proposal is required.");

        riskClassifier.Verify(
            x => x.Classify(It.IsAny<ToolDefinition>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_fail_when_tool_and_action_names_do_not_match()
    {
        var contextAccessor =
            CreateContextAccessor(true);

        var authorizationService =
            CreateAuthorizationService(contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var executor =
            CreateExecutor(
                CreateHandler().Object,
                authorizationService,
                riskClassifier.Object,
                contextAccessor.Object,
                confirmationService.Object);

        var result =
            await executor.ExecuteAsync(
                CreateTool("GetStudentByAdmissionNumber"),
                CreateAction("DeleteStudent"));

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Be("Tool definition and action name do not match.");

        riskClassifier.Verify(
            x => x.Classify(It.IsAny<ToolDefinition>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_fail_when_versions_do_not_match()
    {
        var contextAccessor =
            CreateContextAccessor(true);

        var authorizationService =
            CreateAuthorizationService(contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var executor =
            CreateExecutor(
                CreateHandler().Object,
                authorizationService,
                riskClassifier.Object,
                contextAccessor.Object,
                confirmationService.Object);

        var tool =
            CreateTool(version: 2);

        var action =
            CreateAction(
                tool.Name,
                version: 1);

        var result =
            await executor.ExecuteAsync(
                tool,
                action);

        result.IsFailure.Should().BeTrue();

        riskClassifier.Verify(
            x => x.Classify(It.IsAny<ToolDefinition>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_fail_when_user_is_not_authenticated()
    {
        var contextAccessor =
            CreateContextAccessor(false);

        var authorizationService =
            CreateAuthorizationService(contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var executor =
            CreateExecutor(
                CreateHandler().Object,
                authorizationService,
                riskClassifier.Object,
                contextAccessor.Object,
                confirmationService.Object);

        var tool =
            CreateTool();

        var result =
            await executor.ExecuteAsync(
                tool,
                CreateAction(
                    tool.Name,
                    tool.Version));

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Unauthorized");

        riskClassifier.Verify(
            x => x.Classify(It.IsAny<ToolDefinition>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_fail_when_required_permission_is_missing()
    {
        var contextAccessor =
            CreateContextAccessor(
                authenticated: true);

        var authorizationService =
            CreateAuthorizationService(
                contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var executor =
            CreateExecutor(
                CreateHandler().Object,
                authorizationService,
                riskClassifier.Object,
                contextAccessor.Object,
                confirmationService.Object);

        var tool =
            CreateTool(
                requiredPermission:
                    "Students.Delete");

        var result =
            await executor.ExecuteAsync(
                tool,
                CreateAction(
                    tool.Name,
                    tool.Version));

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Unauthorized");

        riskClassifier.Verify(
            x => x.Classify(It.IsAny<ToolDefinition>()),
            Times.Never);

        confirmationService.Verify(
            x => x.CreatePendingAction(
                It.IsAny<ToolDefinition>(),
                It.IsAny<AiActionProposal>(),
                It.IsAny<AiExecutionContext>(),
                It.IsAny<AiRiskAssessment>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_fail_when_risk_classification_fails()
    {
        var contextAccessor =
            CreateContextAccessor(true);

        var authorizationService =
            CreateAuthorizationService(contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        riskClassifier
            .Setup(x => x.Classify(
                It.IsAny<ToolDefinition>()))
            .Returns(
                Result.Failure<AiRiskAssessment>(
                    Error.Validation(
                        "Risk classification failed.")));

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var executor =
            CreateExecutor(
                CreateHandler().Object,
                authorizationService,
                riskClassifier.Object,
                contextAccessor.Object,
                confirmationService.Object);

        var tool =
            CreateTool();

        var result =
            await executor.ExecuteAsync(
                tool,
                CreateAction(
                    tool.Name,
                    tool.Version));

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Be("Risk classification failed.");

        confirmationService.Verify(
            x => x.CreatePendingAction(
                It.IsAny<ToolDefinition>(),
                It.IsAny<AiActionProposal>(),
                It.IsAny<AiExecutionContext>(),
                It.IsAny<AiRiskAssessment>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_create_pending_confirmation_instead_of_executing_handler()
    {
        var contextAccessor =
            CreateContextAccessor(true);

        var currentContext =
            contextAccessor.Object.GetCurrent();

        var authorizationService =
            CreateAuthorizationService(contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        var tool =
            CreateTool(
                name: "DeleteStudent",
                riskLevel: AiRiskLevel.High);

        var action =
            CreateAction(
                tool.Name,
                tool.Version);

        var assessment =
            new AiRiskAssessment
            {
                ToolName = tool.Name,
                Version = tool.Version,
                RiskLevel = AiRiskLevel.High,
                RequiresConfirmation = true,
                Reasons =
                [
                    "High-risk operation."
                ]
            };

        riskClassifier
            .Setup(x => x.Classify(
                It.IsAny<ToolDefinition>()))
            .Returns(
                Result.Success(assessment));

        var pending =
            new AiPendingAction
            {
                TokenVersion = 1,
                ConfirmationId = Guid.NewGuid(),
                ConfirmationToken = "secure-token",
                ToolName = tool.Name,
                Version = tool.Version,
                RiskLevel = AiRiskLevel.High,
                Arguments = action.Arguments,
                UserId = currentContext.UserId!.Value,
                TenantId = currentContext.TenantId!.Value,
                BranchId = currentContext.BranchId,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                ExpiresAtUtc =
                    DateTimeOffset.UtcNow.AddMinutes(5)
            };

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        confirmationService
            .Setup(x => x.CreatePendingAction(
                It.IsAny<ToolDefinition>(),
                It.IsAny<AiActionProposal>(),
                It.IsAny<AiExecutionContext>(),
                It.IsAny<AiRiskAssessment>()))
            .Returns(
                Result.Success(pending));

        var handler =
            CreateHandler();

        var executor =
            CreateExecutor(
                handler.Object,
                authorizationService,
                riskClassifier.Object,
                contextAccessor.Object,
                confirmationService.Object);

        var result =
            await executor.ExecuteAsync(
                tool,
                action);

        result.IsSuccess.Should().BeTrue();

        result.Value!.Status
            .Should()
            .Be(AiToolExecutionStatus.ConfirmationRequired);

        result.Value.RequiresConfirmation
            .Should()
            .BeTrue();

        result.Value.Confirmation
            .Should()
            .NotBeNull();

        result.Value.Confirmation!.ConfirmationId
            .Should()
            .Be(pending.ConfirmationId);

        result.Value.Confirmation.ConfirmationToken
            .Should()
            .Be("secure-token");

        result.Value.Confirmation.ToolName
            .Should()
            .Be(tool.Name);

        result.Value.Confirmation.Version
            .Should()
            .Be(tool.Version);

        result.Value.Confirmation.RiskLevel
            .Should()
            .Be(AiRiskLevel.High);

        confirmationService.Verify(
            x => x.CreatePendingAction(
                It.Is<ToolDefinition>(
                    t =>
                        t.Name == tool.Name
                        && t.Version == tool.Version),
                It.Is<AiActionProposal>(
                    a =>
                        a.ActionName == action.ActionName
                        && a.Version == action.Version),
                It.Is<AiExecutionContext>(
                    c =>
                        c.UserId == currentContext.UserId
                        && c.TenantId == currentContext.TenantId
                        && c.BranchId == currentContext.BranchId),
                It.Is<AiRiskAssessment>(
                    r =>
                        r.ToolName == assessment.ToolName
                        && r.Version == assessment.Version
                        && r.RiskLevel == assessment.RiskLevel
                        && r.RequiresConfirmation)),
            Times.Once);

        handler.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_return_failure_when_pending_confirmation_creation_fails()
    {
        var contextAccessor =
            CreateContextAccessor(true);

        var authorizationService =
            CreateAuthorizationService(contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        var tool =
            CreateTool(
                name: "DeleteStudent",
                riskLevel: AiRiskLevel.High);

        var assessment =
            new AiRiskAssessment
            {
                ToolName = tool.Name,
                Version = tool.Version,
                RiskLevel = AiRiskLevel.High,
                RequiresConfirmation = true
            };

        riskClassifier
            .Setup(x => x.Classify(
                It.IsAny<ToolDefinition>()))
            .Returns(
                Result.Success(assessment));

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        confirmationService
            .Setup(x => x.CreatePendingAction(
                It.IsAny<ToolDefinition>(),
                It.IsAny<AiActionProposal>(),
                It.IsAny<AiExecutionContext>(),
                It.IsAny<AiRiskAssessment>()))
            .Returns(
                Result.Failure<AiPendingAction>(
                    Error.Unauthorized(
                        "Tenant context could not be established.")));

        var handler =
            CreateHandler();

        var executor =
            CreateExecutor(
                handler.Object,
                authorizationService,
                riskClassifier.Object,
                contextAccessor.Object,
                confirmationService.Object);

        var result =
            await executor.ExecuteAsync(
                tool,
                CreateAction(
                    tool.Name,
                    tool.Version));

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Unauthorized");

        handler.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_execute_immediate_low_risk_tool_without_confirmation_token()
    {
        var contextAccessor =
            CreateContextAccessor(true);

        var authorizationService =
            CreateAuthorizationService(contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        var tool =
            CreateTool();

        riskClassifier
            .Setup(x => x.Classify(
                It.IsAny<ToolDefinition>()))
            .Returns(
                Result.Success(
                    new AiRiskAssessment
                    {
                        ToolName = tool.Name,
                        Version = tool.Version,
                        RiskLevel = AiRiskLevel.Low,
                        RequiresConfirmation = false
                    }));

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var handler =
            CreateHandler();

        handler
            .Setup(x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Success(
                    new AiToolExecutionResult
                    {
                        ToolName = tool.Name,
                        Output = "student"
                    }));

        var executor =
            CreateExecutor(
                handler.Object,
                authorizationService,
                riskClassifier.Object,
                contextAccessor.Object,
                confirmationService.Object);

        var result =
            await executor.ExecuteAsync(
                tool,
                CreateAction(
                    tool.Name,
                    tool.Version));

        result.IsSuccess.Should().BeTrue();

        result.Value!.Status
            .Should()
            .Be(AiToolExecutionStatus.Executed);

        confirmationService.Verify(
            x => x.CreatePendingAction(
                It.IsAny<ToolDefinition>(),
                It.IsAny<AiActionProposal>(),
                It.IsAny<AiExecutionContext>(),
                It.IsAny<AiRiskAssessment>()),
            Times.Never);

        handler.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_fail_when_no_handler_is_registered()
    {
        var contextAccessor =
            CreateContextAccessor(true);

        var authorizationService =
            CreateAuthorizationService(contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        riskClassifier
            .Setup(x => x.Classify(
                It.IsAny<ToolDefinition>()))
            .Returns(
                Result.Success(
                    new AiRiskAssessment
                    {
                        ToolName =
                            "GetStudentByAdmissionNumber",
                        Version = 1,
                        RiskLevel = AiRiskLevel.Low,
                        RequiresConfirmation = false
                    }));

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var executor =
            CreateExecutor(
                Enumerable.Empty<IAiToolHandler>(),
                authorizationService,
                riskClassifier.Object,
                contextAccessor.Object,
                confirmationService.Object);

        var tool =
            CreateTool();

        var result =
            await executor.ExecuteAsync(
                tool,
                CreateAction(
                    tool.Name,
                    tool.Version));

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Contain("No executor is registered");
    }

    [Fact]
    public async Task Should_propagate_handler_failure()
    {
        var contextAccessor =
            CreateContextAccessor(true);

        var authorizationService =
            CreateAuthorizationService(contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        var tool =
            CreateTool();

        riskClassifier
            .Setup(x => x.Classify(
                It.IsAny<ToolDefinition>()))
            .Returns(
                Result.Success(
                    new AiRiskAssessment
                    {
                        ToolName = tool.Name,
                        Version = tool.Version,
                        RiskLevel = AiRiskLevel.Low,
                        RequiresConfirmation = false
                    }));

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var handler =
            CreateHandler();

        handler
            .Setup(x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure<AiToolExecutionResult>(
                    Error.Validation(
                        "Tool execution failed.")));

        var executor =
            CreateExecutor(
                handler.Object,
                authorizationService,
                riskClassifier.Object,
                contextAccessor.Object,
                confirmationService.Object);

        var result =
            await executor.ExecuteAsync(
                tool,
                CreateAction(
                    tool.Name,
                    tool.Version));

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Be("Tool execution failed.");
    }

    [Fact]
    public async Task Should_pass_cancellation_token_to_handler()
    {
        var contextAccessor =
            CreateContextAccessor(true);

        var authorizationService =
            CreateAuthorizationService(contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        var tool =
            CreateTool();

        riskClassifier
            .Setup(x => x.Classify(
                It.IsAny<ToolDefinition>()))
            .Returns(
                Result.Success(
                    new AiRiskAssessment
                    {
                        ToolName = tool.Name,
                        Version = tool.Version,
                        RiskLevel = AiRiskLevel.Low,
                        RequiresConfirmation = false
                    }));

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var handler =
            CreateHandler();

        var expectedToken =
            new CancellationTokenSource().Token;

        CancellationToken capturedToken = default;

        handler
            .Setup(x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()))
            .Callback<
                IReadOnlyDictionary<string, JsonElement>,
                CancellationToken>(
                (_, token) => capturedToken = token)
            .ReturnsAsync(
                Result.Success(
                    new AiToolExecutionResult
                    {
                        ToolName = tool.Name
                    }));

        var executor =
            CreateExecutor(
                handler.Object,
                authorizationService,
                riskClassifier.Object,
                contextAccessor.Object,
                confirmationService.Object);

        await executor.ExecuteAsync(
            tool,
            CreateAction(
                tool.Name,
                tool.Version),
            expectedToken);

        capturedToken
            .Should()
            .Be(expectedToken);
    }

    private static AiToolExecutor CreateExecutor(
        IAiToolHandler handler,
        AiToolAuthorizationService authorizationService,
        IAiRiskClassifier riskClassifier,
        IAiExecutionContextAccessor contextAccessor,
        IAiConfirmationTokenService confirmationTokenService)
    {
        return new AiToolExecutor(
            [handler],
            authorizationService,
            riskClassifier,
            contextAccessor,
            confirmationTokenService);
    }

    private static AiToolExecutor CreateExecutor(
        IEnumerable<IAiToolHandler> handlers,
        AiToolAuthorizationService authorizationService,
        IAiRiskClassifier riskClassifier,
        IAiExecutionContextAccessor contextAccessor,
        IAiConfirmationTokenService confirmationTokenService)
    {
        return new AiToolExecutor(
            handlers,
            authorizationService,
            riskClassifier,
            contextAccessor,
            confirmationTokenService);
    }

    private static Mock<IAiToolHandler> CreateHandler()
    {
        var mock =
            new Mock<IAiToolHandler>();

        mock.SetupGet(x => x.Name)
            .Returns(
                "GetStudentByAdmissionNumber");

        mock.SetupGet(x => x.Version)
            .Returns(1);

        return mock;
    }

    private static Mock<IAiExecutionContextAccessor>
        CreateContextAccessor(
            bool authenticated)
    {
        var context =
            new AiExecutionContext
            {
                IsAuthenticated = authenticated,
                UserId = authenticated
                    ? Guid.NewGuid()
                    : null,
                TenantId = authenticated
                    ? Guid.NewGuid()
                    : null,
                BranchId = authenticated
                    ? Guid.NewGuid()
                    : null,
                Permissions =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase)
            };

        var mock =
            new Mock<IAiExecutionContextAccessor>();

        mock.Setup(x => x.GetCurrent())
            .Returns(context);

        return mock;
    }

    private static AiToolAuthorizationService
    CreateAuthorizationService(
        Mock<IAiExecutionContextAccessor>
            contextAccessor)
    {
        return new AiToolAuthorizationService(
            contextAccessor.Object,
            new AiToolScopeAuthorizationService(
                contextAccessor.Object));
    }

    private static ToolDefinition CreateTool(
        string name =
            "GetStudentByAdmissionNumber",
        int version = 1,
        string? requiredPermission = null,
        AiRiskLevel riskLevel =
            AiRiskLevel.Low)
    {
        return new ToolDefinition
        {
            Name = name,
            Description = $"Test tool: {name}",
            Version = version,
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
                        new JsonArray(
                            "admissionNumber"),
                    ["additionalProperties"] = false
                },
            RequiredPermission =
                requiredPermission,
            DataScope =
                AiDataScope.None,
            RiskLevel =
                riskLevel,
            ConfirmationPolicy =
                AiConfirmationPolicy.None,
            AuditPolicy =
                AiAuditPolicy.Required
        };
    }

    private static AiActionProposal CreateAction(
        string actionName,
        int version = 1)
    {
        return new AiActionProposal
        {
            ActionName = actionName,
            Version = version,
            Arguments =
                new Dictionary<string, JsonElement>
                {
                    ["admissionNumber"] =
                        JsonSerializer.SerializeToElement(
                            "ADM-001")
                }
        };
    }
}