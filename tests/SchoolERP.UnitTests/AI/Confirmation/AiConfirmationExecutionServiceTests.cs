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

namespace SchoolERP.UnitTests.Features.AI.Confirmation;

public sealed class AiConfirmationExecutionServiceTests
{
    [Fact]
    public async Task Should_fail_when_confirmation_token_is_invalid()
    {
        var tokenService =
            new Mock<IAiConfirmationTokenService>();

        tokenService
            .Setup(x => x.ReadToken("invalid-token"))
            .Returns(
                Result.Failure<AiPendingAction>(
                    Error.Unauthorized(
                        "Invalid or tampered AI confirmation token.")));

        var service =
            CreateService(
                tokenService: tokenService.Object);

        var result =
            await service.ExecuteAsync(
                "invalid-token");

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Unauthorized");
    }

    [Fact]
    public async Task Should_fail_when_user_is_not_authenticated()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                new AiExecutionContext
                {
                    IsAuthenticated = false
                });

        var service =
            CreateService(
                tokenService: tokenService.Object,
                contextAccessor:
                    contextAccessor.Object);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Contain("Authenticated user context");
    }

    [Fact]
    public async Task Should_fail_when_user_context_does_not_match()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                CreateContext(
                    userId: Guid.NewGuid(),
                    tenantId: pending.TenantId,
                    branchId: pending.BranchId));

        var service =
            CreateService(
                tokenService: tokenService.Object,
                contextAccessor:
                    contextAccessor.Object);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Unauthorized");

        result.Error.Message
            .Should()
            .Contain("current user");
    }

    [Fact]
    public async Task Should_fail_when_tenant_context_does_not_match()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                CreateContext(
                    userId: pending.UserId,
                    tenantId: Guid.NewGuid(),
                    branchId: pending.BranchId));

        var service =
            CreateService(
                tokenService: tokenService.Object,
                contextAccessor:
                    contextAccessor.Object);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Contain("current tenant");
    }

    [Fact]
    public async Task Should_fail_when_branch_context_does_not_match()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                CreateContext(
                    userId: pending.UserId,
                    tenantId: pending.TenantId,
                    branchId: Guid.NewGuid()));

        var service =
            CreateService(
                tokenService: tokenService.Object,
                contextAccessor:
                    contextAccessor.Object);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Contain("current branch");
    }

    [Fact]
    public async Task Should_fail_when_tool_no_longer_exists()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                CreateContextFromPending(pending));

        var registry =
            new Mock<IAiToolRegistry>();

        registry
            .Setup(x => x.Get(
                pending.ToolName,
                pending.Version))
            .Returns((ToolDefinition?)null);

        var service =
            CreateService(
                tokenService: tokenService.Object,
                contextAccessor:
                    contextAccessor.Object,
                registry:
                    registry.Object);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("NotFound");
    }

    [Fact]
    public async Task Should_recheck_authorization()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                CreateContextFromPending(pending));

        var tool =
            CreateTool(
                requiredPermission:
                    "Students.Delete");

        var registry =
            CreateRegistry(tool);

        var service =
            CreateService(
                tokenService: tokenService.Object,
                contextAccessor:
                    contextAccessor.Object,
                registry:
                    registry.Object,
                permissions: []);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Unauthorized");
    }

    [Fact]
    public async Task Should_fail_when_risk_reclassification_fails()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                CreateContextFromPending(pending));

        var tool =
            CreateTool();

        var registry =
            CreateRegistry(tool);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        riskClassifier
            .Setup(x => x.Classify(tool))
            .Returns(
                Result.Failure<AiRiskAssessment>(
                    Error.Validation(
                        "Risk policy unavailable.")));

        var service =
            CreateService(
                tokenService: tokenService.Object,
                contextAccessor:
                    contextAccessor.Object,
                registry:
                    registry.Object,
                riskClassifier:
                    riskClassifier.Object);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Be("Risk policy unavailable.");
    }

    [Fact]
    public async Task Should_fail_when_confirmation_is_no_longer_required()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                CreateContextFromPending(pending));

        var tool =
            CreateTool();

        var registry =
            CreateRegistry(tool);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        riskClassifier
            .Setup(x => x.Classify(tool))
            .Returns(
                Result.Success(
                    new AiRiskAssessment
                    {
                        ToolName = tool.Name,
                        Version = tool.Version,
                        RiskLevel = pending.RiskLevel,
                        RequiresConfirmation = false
                    }));

        var service =
            CreateService(
                tokenService: tokenService.Object,
                contextAccessor:
                    contextAccessor.Object,
                registry:
                    registry.Object,
                riskClassifier:
                    riskClassifier.Object);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Contain("no longer valid");
    }

    [Fact]
    public async Task Should_fail_when_risk_level_changed()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                CreateContextFromPending(pending));

        var tool =
            CreateTool();

        var registry =
            CreateRegistry(tool);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        riskClassifier
            .Setup(x => x.Classify(tool))
            .Returns(
                Result.Success(
                    new AiRiskAssessment
                    {
                        ToolName = tool.Name,
                        Version = tool.Version,
                        RiskLevel = AiRiskLevel.Critical,
                        RequiresConfirmation = true
                    }));

        var service =
            CreateService(
                tokenService: tokenService.Object,
                contextAccessor:
                    contextAccessor.Object,
                registry:
                    registry.Object,
                riskClassifier:
                    riskClassifier.Object);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Unauthorized");

        result.Error.Message
            .Should()
            .Contain("risk policy");
    }

    [Fact]
    public async Task Should_fail_when_handler_is_missing()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                CreateContextFromPending(pending));

        var tool =
            CreateTool();

        var registry =
            CreateRegistry(tool);

        var riskClassifier =
            CreateRiskClassifier(
                tool,
                pending.RiskLevel);

        var service =
            CreateService(
                tokenService: tokenService.Object,
                contextAccessor:
                    contextAccessor.Object,
                registry:
                    registry.Object,
                riskClassifier:
                    riskClassifier.Object,
                handlers:
                    []);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Validation");

        result.Error.Message
            .Should()
            .Contain("No executor is registered");

        var store =
            new Mock<IAiConfirmationTokenStore>();

        store.Verify(
            x => x.TryConsumeAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_consume_token_and_execute_handler()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                CreateContextFromPending(pending));

        var tool =
            CreateTool();

        var registry =
            CreateRegistry(tool);

        var riskClassifier =
            CreateRiskClassifier(
                tool,
                pending.RiskLevel);

        var store =
            new Mock<IAiConfirmationTokenStore>();

        store
            .Setup(x => x.TryConsumeAsync(
                pending.ConfirmationId,
                pending.ExpiresAtUtc,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler =
            CreateHandler(tool);

        var service =
            CreateService(
                tokenService: tokenService.Object,
                tokenStore:
                    store.Object,
                contextAccessor:
                    contextAccessor.Object,
                registry:
                    registry.Object,
                riskClassifier:
                    riskClassifier.Object,
                handlers:
                    [handler.Object]);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsSuccess.Should().BeTrue();

        result.Value!.ToolName
            .Should()
            .Be(tool.Name);

        store.Verify(
            x => x.TryConsumeAsync(
                pending.ConfirmationId,
                pending.ExpiresAtUtc,
                It.IsAny<CancellationToken>()),
            Times.Once);

        handler.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_not_execute_when_token_consumption_fails()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                CreateContextFromPending(pending));

        var tool =
            CreateTool();

        var registry =
            CreateRegistry(tool);

        var riskClassifier =
            CreateRiskClassifier(
                tool,
                pending.RiskLevel);

        var store =
            new Mock<IAiConfirmationTokenStore>();

        store
            .Setup(x => x.TryConsumeAsync(
                pending.ConfirmationId,
                pending.ExpiresAtUtc,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler =
            CreateHandler(tool);

        var service =
            CreateService(
                tokenService: tokenService.Object,
                tokenStore:
                    store.Object,
                contextAccessor:
                    contextAccessor.Object,
                registry:
                    registry.Object,
                riskClassifier:
                    riskClassifier.Object,
                handlers:
                    [handler.Object]);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

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
    public async Task Should_pass_exact_pending_arguments_to_handler()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                CreateContextFromPending(pending));

        var tool =
            CreateTool();

        var registry =
            CreateRegistry(tool);

        var riskClassifier =
            CreateRiskClassifier(
                tool,
                pending.RiskLevel);

        var store =
            new Mock<IAiConfirmationTokenStore>();

        store
            .Setup(x => x.TryConsumeAsync(
                pending.ConfirmationId,
                pending.ExpiresAtUtc,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler =
            CreateHandler(tool);

        IReadOnlyDictionary<string, JsonElement>? capturedArguments =
            null;

        handler
            .Setup(x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()))
            .Callback<
                IReadOnlyDictionary<string, JsonElement>,
                CancellationToken>(
                (arguments, _) =>
                    capturedArguments = arguments)
            .ReturnsAsync(
                Result.Success(
                    new AiToolExecutionResult
                    {
                        ToolName = tool.Name
                    }));

        var service =
            CreateService(
                tokenService: tokenService.Object,
                tokenStore:
                    store.Object,
                contextAccessor:
                    contextAccessor.Object,
                registry:
                    registry.Object,
                riskClassifier:
                    riskClassifier.Object,
                handlers:
                    [handler.Object]);

        await service.ExecuteAsync(
            pending.ConfirmationToken);

        capturedArguments
            .Should()
            .NotBeNull();

        capturedArguments!
            .Should()
            .ContainKey("admissionNumber");

        capturedArguments["admissionNumber"]
            .GetString()
            .Should()
            .Be("ADM-001");
    }

    [Fact]
    public async Task Should_keep_token_consumed_when_handler_fails()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateTokenService(pending);

        var contextAccessor =
            CreateContextAccessor(
                CreateContextFromPending(pending));

        var tool =
            CreateTool();

        var registry =
            CreateRegistry(tool);

        var riskClassifier =
            CreateRiskClassifier(
                tool,
                pending.RiskLevel);

        var store =
            new Mock<IAiConfirmationTokenStore>();

        store
            .Setup(x => x.TryConsumeAsync(
                pending.ConfirmationId,
                pending.ExpiresAtUtc,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler =
            CreateHandler(tool);

        handler
            .Setup(x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure<AiToolExecutionResult>(
                    Error.Validation(
                        "Tool execution failed.")));

        var service =
            CreateService(
                tokenService: tokenService.Object,
                tokenStore:
                    store.Object,
                contextAccessor:
                    contextAccessor.Object,
                registry:
                    registry.Object,
                riskClassifier:
                    riskClassifier.Object,
                handlers:
                    [handler.Object]);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Be("Tool execution failed.");

        store.Verify(
            x => x.TryConsumeAsync(
                pending.ConfirmationId,
                pending.ExpiresAtUtc,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_fail_before_consume_when_cancellation_is_requested()
    {
        var pending =
            CreatePendingAction();

        var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        var tokenService =
            CreateTokenService(pending);

        var store =
            new Mock<IAiConfirmationTokenStore>();

        var service =
            CreateService(
                tokenService:
                    tokenService.Object,
                tokenStore:
                    store.Object);

        Func<Task> action =
            () =>
                service.ExecuteAsync(
                    pending.ConfirmationToken,
                    cancellationTokenSource.Token);

        await action
            .Should()
            .ThrowAsync<OperationCanceledException>();

        store.Verify(
            x => x.TryConsumeAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static AiConfirmationExecutionService CreateService(
    IAiConfirmationTokenService? tokenService = null,
    IAiConfirmationTokenStore? tokenStore = null,
    IAiExecutionContextAccessor? contextAccessor = null,
    IAiToolRegistry? registry = null,
    IAiRiskClassifier? riskClassifier = null,
    IEnumerable<IAiToolHandler>? handlers = null,
    IEnumerable<string>? permissions = null)
    {
        if (contextAccessor is null)
        {
            contextAccessor =
                CreateContextAccessor(
                    CreateContext(
                        permissions:
                            permissions ?? []))
                .Object;
        }

        var authorizationService =
     new AiToolAuthorizationService(
         contextAccessor,
         new AiToolScopeAuthorizationService(
             contextAccessor));

        return new AiConfirmationExecutionService(
            tokenService
                ?? new Mock<IAiConfirmationTokenService>()
                    .Object,
            tokenStore
                ?? new Mock<IAiConfirmationTokenStore>()
                    .Object,
            contextAccessor,
            registry
                ?? new Mock<IAiToolRegistry>()
                    .Object,
            authorizationService,
            riskClassifier
                ?? CreateRiskClassifier(
                    CreateTool(),
                    AiRiskLevel.High).Object,
            handlers
                ?? []);
    }

    private static Mock<IAiConfirmationTokenService>
        CreateTokenService(
            AiPendingAction pending)
    {
        var mock =
            new Mock<IAiConfirmationTokenService>();

        mock.Setup(x => x.ReadToken(
                pending.ConfirmationToken))
            .Returns(
                Result.Success(pending));

        return mock;
    }

    private static Mock<IAiExecutionContextAccessor>
        CreateContextAccessor(
            AiExecutionContext context)
    {
        var mock =
            new Mock<IAiExecutionContextAccessor>();

        mock.Setup(x => x.GetCurrent())
            .Returns(context);

        return mock;
    }

    private static Mock<IAiToolRegistry>
        CreateRegistry(
            ToolDefinition tool)
    {
        var mock =
            new Mock<IAiToolRegistry>();

        mock.Setup(x => x.Get(
                tool.Name,
                tool.Version))
            .Returns(tool);

        return mock;
    }

    private static Mock<IAiRiskClassifier>
        CreateRiskClassifier(
            ToolDefinition tool,
            AiRiskLevel riskLevel)
    {
        var mock =
            new Mock<IAiRiskClassifier>();

        mock.Setup(x => x.Classify(tool))
            .Returns(
                Result.Success(
                    new AiRiskAssessment
                    {
                        ToolName = tool.Name,
                        Version = tool.Version,
                        RiskLevel = riskLevel,
                        RequiresConfirmation = true,
                        Reasons =
                        [
                            "Confirmation required."
                        ]
                    }));

        return mock;
    }

    private static Mock<IAiToolHandler>
        CreateHandler(
            ToolDefinition tool)
    {
        var mock =
            new Mock<IAiToolHandler>();

        mock.SetupGet(x => x.Name)
            .Returns(tool.Name);

        mock.SetupGet(x => x.Version)
            .Returns(tool.Version);

        mock.Setup(x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Success(
                    new AiToolExecutionResult
                    {
                        ToolName = tool.Name,
                        Output = "executed"
                    }));

        return mock;
    }

    private static AiPendingAction CreatePendingAction()
    {
        var tool =
            CreateTool();

        return new AiPendingAction
        {
            TokenVersion = 1,
            ConfirmationId = Guid.NewGuid(),
            ConfirmationToken = "confirmation-token",
            ToolName = tool.Name,
            Version = tool.Version,
            RiskLevel = AiRiskLevel.High,
            Arguments =
                new Dictionary<string, JsonElement>
                {
                    ["admissionNumber"] =
                        JsonSerializer.SerializeToElement(
                            "ADM-001")
                },
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            CreatedAtUtc =
                DateTimeOffset.UtcNow.AddMinutes(-1),
            ExpiresAtUtc =
                DateTimeOffset.UtcNow.AddMinutes(5)
        };
    }

    private static AiExecutionContext
        CreateContextFromPending(
            AiPendingAction pending)
    {
        return CreateContext(
            userId: pending.UserId,
            tenantId: pending.TenantId,
            branchId: pending.BranchId);
    }

    private static AiExecutionContext CreateContext(
        Guid? userId = null,
        Guid? tenantId = null,
        Guid? branchId = null,
        IEnumerable<string>? permissions = null)
    {
        return new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = userId ?? Guid.NewGuid(),
            TenantId = tenantId ?? Guid.NewGuid(),
            BranchId = branchId ?? Guid.NewGuid(),
            Permissions =
                new HashSet<string>(
                    permissions ?? [],
                    StringComparer.OrdinalIgnoreCase)
        };
    }

    private static ToolDefinition CreateTool(
        string name =
            "GetStudentByAdmissionNumber",
        int version = 1,
        string? requiredPermission = null)
    {
        return new ToolDefinition
        {
            Name = name,
            Description = "Test AI tool.",
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
                    ["additionalProperties"] =
                        false
                },
            RequiredPermission =
                requiredPermission,
            DataScope =
                AiDataScope.None,
            RiskLevel =
                AiRiskLevel.High,
            ConfirmationPolicy =
                AiConfirmationPolicy.Required,
            AuditPolicy =
                AiAuditPolicy.Required
        };
    }
}