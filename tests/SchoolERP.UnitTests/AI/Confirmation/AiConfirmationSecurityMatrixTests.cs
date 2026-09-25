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

public sealed class AiConfirmationSecurityMatrixTests
{
    [Fact]
    public async Task Low_risk_without_confirmation_should_execute_immediately()
    {
        var tool =
            CreateTool(
                riskLevel: AiRiskLevel.Low,
                confirmationPolicy:
                    AiConfirmationPolicy.None);

        var handler =
            CreateHandler(tool);

        var riskClassifier =
            CreateRiskClassifier(
                tool,
                AiRiskLevel.Low,
                requiresConfirmation: false);

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var contextAccessor =
            CreateContextAccessor();

        var authorization =
            CreateAuthorizationService(
                contextAccessor);

        var executor =
            new AiToolExecutor(
                [handler.Object],
                authorization,
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

        result.Value.RequiresConfirmation
            .Should()
            .BeFalse();

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
    public async Task Low_risk_with_required_policy_should_request_confirmation()
    {
        var tool =
            CreateTool(
                riskLevel: AiRiskLevel.Low,
                confirmationPolicy:
                    AiConfirmationPolicy.Required);

        var pending =
            CreatePendingAction(
                tool,
                AiRiskLevel.Low);

        var riskClassifier =
            CreateRiskClassifier(
                tool,
                AiRiskLevel.Low,
                requiresConfirmation: true);

        var confirmationService =
            CreateConfirmationTokenService(
                pending);

        var contextAccessor =
            CreateContextAccessor();

        var authorization =
            CreateAuthorizationService(
                contextAccessor);

        var handler =
            CreateHandler(tool);

        var executor =
            new AiToolExecutor(
                [handler.Object],
                authorization,
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
            .Be(AiToolExecutionStatus.ConfirmationRequired);

        result.Value.Confirmation
            .Should()
            .NotBeNull();

        handler.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(AiRiskLevel.High)]
    [InlineData(AiRiskLevel.Critical)]
    public async Task High_and_critical_risk_should_require_confirmation(
        AiRiskLevel riskLevel)
    {
        var tool =
            CreateTool(
                riskLevel: riskLevel,
                confirmationPolicy:
                    AiConfirmationPolicy.None);

        var pending =
            CreatePendingAction(
                tool,
                riskLevel);

        var riskClassifier =
            CreateRiskClassifier(
                tool,
                riskLevel,
                requiresConfirmation: true);

        var confirmationService =
            CreateConfirmationTokenService(
                pending);

        var contextAccessor =
            CreateContextAccessor();

        var authorization =
            CreateAuthorizationService(
                contextAccessor);

        var handler =
            CreateHandler(tool);

        var executor =
            new AiToolExecutor(
                [handler.Object],
                authorization,
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

        result.Value!.RequiresConfirmation
            .Should()
            .BeTrue();

        result.Value.Confirmation!.RiskLevel
            .Should()
            .Be(riskLevel);

        handler.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Unauthenticated_user_should_never_reach_risk_stage()
    {
        var tool =
            CreateTool(
                riskLevel: AiRiskLevel.High);

        var contextAccessor =
            CreateContextAccessor(
                authenticated: false);

        var authorization =
            CreateAuthorizationService(
                contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        var handler =
            CreateHandler(tool);

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var executor =
            new AiToolExecutor(
                [handler.Object],
                authorization,
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

        riskClassifier.Verify(
            x => x.Classify(
                It.IsAny<ToolDefinition>()),
            Times.Never);

        handler.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Permission_failure_should_prevent_confirmation_creation()
    {
        var tool =
            CreateTool(
                riskLevel: AiRiskLevel.High,
                requiredPermission:
                    "Students.Delete");

        var contextAccessor =
            CreateContextAccessor(
                permissions: []);

        var authorization =
            CreateAuthorizationService(
                contextAccessor);

        var riskClassifier =
            new Mock<IAiRiskClassifier>();

        var confirmationService =
            new Mock<IAiConfirmationTokenService>();

        var handler =
            CreateHandler(tool);

        var executor =
            new AiToolExecutor(
                [handler.Object],
                authorization,
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

        riskClassifier.Verify(
            x => x.Classify(
                It.IsAny<ToolDefinition>()),
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
    public async Task Confirmation_should_be_bound_to_current_user()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateConfirmationTokenService(
                pending);

        var wrongUserContext =
            CreateContext(
                userId:
                    Guid.NewGuid(),
                tenantId:
                    pending.TenantId,
                branchId:
                    pending.BranchId);

        var contextAccessor =
            CreateContextAccessor(
                context:
                    wrongUserContext);

        var tool =
            CreateTool(
                name: pending.ToolName,
                version: pending.Version,
                riskLevel: pending.RiskLevel);

        var service =
            CreateConfirmationExecutionService(
                pending,
                tokenService,
                contextAccessor,
                tool);

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
    public async Task Confirmation_should_be_bound_to_current_tenant()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateConfirmationTokenService(
                pending);

        var wrongTenantContext =
            CreateContext(
                userId:
                    pending.UserId,
                tenantId:
                    Guid.NewGuid(),
                branchId:
                    pending.BranchId);

        var contextAccessor =
            CreateContextAccessor(
    context:
        CreateContextFromPending(pending));

        var tool =
            CreateTool(
                name: pending.ToolName,
                version: pending.Version,
                riskLevel: pending.RiskLevel);

        var service =
            CreateConfirmationExecutionService(
                pending,
                tokenService,
                contextAccessor,
                tool);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();

        result.Error.Message
            .Should()
            .Contain("current tenant");
    }

    [Fact]
    public async Task Confirmation_should_be_bound_to_current_branch()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateConfirmationTokenService(
                pending);

        var wrongBranchContext =
            CreateContext(
                userId:
                    pending.UserId,
                tenantId:
                    pending.TenantId,
                branchId:
                    Guid.NewGuid());

        var contextAccessor =
          CreateContextAccessor(
    context:
        CreateContextFromPending(pending));

        var tool =
            CreateTool(
                name: pending.ToolName,
                version: pending.Version,
                riskLevel: pending.RiskLevel);

        var service =
            CreateConfirmationExecutionService(
                pending,
                tokenService,
                contextAccessor,
                tool);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();

        result.Error.Message
            .Should()
            .Contain("current branch");
    }

    [Fact]
    public async Task Expired_token_should_not_execute()
    {
        var pending =
            CreatePendingAction(
                expiresAtUtc:
                    DateTimeOffset.UtcNow.AddSeconds(-1));

        var tokenService =
            new Mock<IAiConfirmationTokenService>();

        tokenService
            .Setup(x => x.ReadToken(
                pending.ConfirmationToken))
            .Returns(
                Result.Failure<AiPendingAction>(
                    Error.Unauthorized(
                        "AI confirmation token has expired.")));

        var service =
            CreateConfirmationExecutionService(
                pending,
                tokenService);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();

        result.Error.Code
            .Should()
            .Be("Unauthorized");
    }

    [Fact]
    public async Task Used_token_should_not_execute_again()
    {
        var pending =
            CreatePendingAction();

        var tokenService =
            CreateConfirmationTokenService(
                pending);

        var contextAccessor =
           CreateContextAccessor(
    context:
        CreateContextFromPending(pending));

        var tool =
            CreateTool(
                name: pending.ToolName,
                version: pending.Version,
                riskLevel: pending.RiskLevel);

        var registry =
            new Mock<IAiToolRegistry>();

        registry
            .Setup(x => x.Get(
                tool.Name,
                tool.Version))
            .Returns(tool);

        var riskClassifier =
            CreateRiskClassifier(
                tool,
                pending.RiskLevel,
                requiresConfirmation: true);

        var authorization =
            CreateAuthorizationService(
                contextAccessor);

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
            new AiConfirmationExecutionService(
                tokenService.Object,
                store.Object,
                contextAccessor.Object,
                registry.Object,
                authorization,
                riskClassifier.Object,
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
    public async Task Changed_risk_level_should_invalidate_confirmation()
    {
        var pending =
            CreatePendingAction(
                riskLevel:
                    AiRiskLevel.High);

        var tokenService =
            CreateConfirmationTokenService(
                pending);

        var contextAccessor =
            CreateContextAccessor(
                context:
                    CreateContextFromPending(
                            pending));

        var tool =
            CreateTool(
                name: pending.ToolName,
                version: pending.Version,
                riskLevel:
                    AiRiskLevel.Critical);

        var registry =
            new Mock<IAiToolRegistry>();

        registry
            .Setup(x => x.Get(
                tool.Name,
                tool.Version))
            .Returns(tool);

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
                        RiskLevel =
                            AiRiskLevel.Critical,
                        RequiresConfirmation = true
                    }));

        var context =
            contextAccessor.Object;

        var service =
            new AiConfirmationExecutionService(
                tokenService.Object,
                new Mock<IAiConfirmationTokenStore>()
                    .Object,
                contextAccessor.Object,
                registry.Object,
                CreateAuthorizationService(
                    contextAccessor),
                riskClassifier.Object,
                []);

        var result =
            await service.ExecuteAsync(
                pending.ConfirmationToken);

        result.IsFailure.Should().BeTrue();

        result.Error.Message
            .Should()
            .Contain("risk policy");

        _ = context;
    }

    private static AiConfirmationExecutionService
        CreateConfirmationExecutionService(
            AiPendingAction pending,
            Mock<IAiConfirmationTokenService>? tokenService = null,
            Mock<IAiExecutionContextAccessor>? contextAccessor = null,
            ToolDefinition? tool = null)
    {
        tokenService ??=
            CreateConfirmationTokenService(
                pending);

        contextAccessor ??=
           CreateContextAccessor(
    context:
        CreateContextFromPending(pending));

        tool ??=
            CreateTool(
                name: pending.ToolName,
                version: pending.Version,
                riskLevel: pending.RiskLevel);

        var registry =
            new Mock<IAiToolRegistry>();

        registry
            .Setup(x => x.Get(
                tool.Name,
                tool.Version))
            .Returns(tool);

        var riskClassifier =
            CreateRiskClassifier(
                tool,
                pending.RiskLevel,
                requiresConfirmation: true);

        var handler =
            CreateHandler(tool);

        var store =
            new Mock<IAiConfirmationTokenStore>();

        store
            .Setup(x => x.TryConsumeAsync(
                pending.ConfirmationId,
                pending.ExpiresAtUtc,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        return new AiConfirmationExecutionService(
            tokenService.Object,
            store.Object,
            contextAccessor.Object,
            registry.Object,
            CreateAuthorizationService(
                contextAccessor),
            riskClassifier.Object,
            [handler.Object]);
    }

    private static Mock<IAiConfirmationTokenService>
        CreateConfirmationTokenService(
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

    private static Mock<IAiRiskClassifier>
        CreateRiskClassifier(
            ToolDefinition tool,
            AiRiskLevel riskLevel,
            bool requiresConfirmation)
    {
        var mock =
            new Mock<IAiRiskClassifier>();

        mock.Setup(x => x.Classify(tool))
            .Returns(
                Result.Success(
                    new AiRiskAssessment
                    {
                        ToolName =
                            tool.Name,
                        Version =
                            tool.Version,
                        RiskLevel =
                            riskLevel,
                        RequiresConfirmation =
                            requiresConfirmation,
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
                        ToolName =
                            tool.Name,
                        Output =
                            "executed"
                    }));

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

    private static Mock<IAiExecutionContextAccessor>
        CreateContextAccessor(
            bool authenticated = true,
            IEnumerable<string>? permissions = null,
            AiExecutionContext? context = null)
    {
        context ??=
            CreateContext(
                authenticated:
                    authenticated,
                permissions:
                    permissions);

        var mock =
            new Mock<IAiExecutionContextAccessor>();

        mock.Setup(x => x.GetCurrent())
            .Returns(context);

        return mock;
    }

    private static AiExecutionContext
        CreateContext(
            bool authenticated = true,
            Guid? userId = null,
            Guid? tenantId = null,
            Guid? branchId = null,
            IEnumerable<string>? permissions = null)
    {
        return new AiExecutionContext
        {
            IsAuthenticated =
                authenticated,
            UserId =
                authenticated
                    ? userId ?? Guid.NewGuid()
                    : null,
            TenantId =
                authenticated
                    ? tenantId ?? Guid.NewGuid()
                    : null,
            BranchId =
                authenticated
                    ? branchId ?? Guid.NewGuid()
                    : null,
            Permissions =
                new HashSet<string>(
                    permissions ?? [],
                    StringComparer.OrdinalIgnoreCase)
        };
    }

    private static AiExecutionContext
        CreateContextFromPending(
            AiPendingAction pending)
    {
        return CreateContext(
            userId:
                pending.UserId,
            tenantId:
                pending.TenantId,
            branchId:
                pending.BranchId);
    }

    private static ToolDefinition
        CreateTool(
            string name =
                "GetStudentByAdmissionNumber",
            int version = 1,
            AiRiskLevel riskLevel =
                AiRiskLevel.Low,
            AiConfirmationPolicy
                confirmationPolicy =
                    AiConfirmationPolicy.None,
            string? requiredPermission = null)
    {
        return new ToolDefinition
        {
            Name =
                name,
            Description =
                "Test AI tool.",
            Version =
                version,
            InputSchema =
                new JsonObject
                {
                    ["type"] =
                        "object",
                    ["properties"] =
                        new JsonObject
                        {
                            ["admissionNumber"] =
                                new JsonObject
                                {
                                    ["type"] =
                                        "string"
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
                riskLevel,
            ConfirmationPolicy =
                confirmationPolicy,
            AuditPolicy =
                AiAuditPolicy.Required
        };
    }

    private static AiActionProposal
        CreateAction(
            string actionName,
            int version = 1)
    {
        return new AiActionProposal
        {
            ActionName =
                actionName,
            Version =
                version,
            Arguments =
                new Dictionary<string, JsonElement>
                {
                    ["admissionNumber"] =
                        JsonSerializer
                            .SerializeToElement(
                                "ADM-001")
                }
        };
    }

    private static AiPendingAction
        CreatePendingAction(
            ToolDefinition? tool = null,
            AiRiskLevel riskLevel =
                AiRiskLevel.High,
            DateTimeOffset?
                expiresAtUtc = null)
    {
        tool ??=
            CreateTool(
                riskLevel:
                    riskLevel);

        var userId =
            Guid.NewGuid();

        var tenantId =
            Guid.NewGuid();

        var branchId =
            Guid.NewGuid();

        var createdAt =
            DateTimeOffset.UtcNow
                .AddMinutes(-1);

        return new AiPendingAction
        {
            TokenVersion =
                1,
            ConfirmationId =
                Guid.NewGuid(),
            ConfirmationToken =
                "confirmation-token",
            ToolName =
                tool.Name,
            Version =
                tool.Version,
            RiskLevel =
                riskLevel,
            Arguments =
                new Dictionary<string, JsonElement>
                {
                    ["admissionNumber"] =
                        JsonSerializer
                            .SerializeToElement(
                                "ADM-001")
                },
            UserId =
                userId,
            TenantId =
                tenantId,
            BranchId =
                branchId,
            CreatedAtUtc =
                createdAt,
            ExpiresAtUtc =
                expiresAtUtc
                ?? DateTimeOffset.UtcNow
                    .AddMinutes(5)
        };
    }
}