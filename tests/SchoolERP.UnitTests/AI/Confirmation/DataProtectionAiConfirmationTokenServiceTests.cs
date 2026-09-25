using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using SchoolERP.Application.Features.AI.Confirmation;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Risk;
using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Infrastructure.Services.AI.Confirmation;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Confirmation;

public sealed class DataProtectionAiConfirmationTokenServiceTests
{
    [Fact]
    public void Should_create_and_read_confirmation_token()
    {
        var service =
            CreateService();

        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        var context =
            new AiExecutionContext
            {
                IsAuthenticated = true,
                UserId = userId,
                TenantId = tenantId,
                BranchId = branchId,
                Permissions =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase)
            };

        var tool =
            CreateTool();

        var action =
            CreateAction(
                tool.Name,
                tool.Version);

        var assessment =
            CreateRiskAssessment(
                tool,
                requiresConfirmation: true);

        var createResult =
            service.CreatePendingAction(
                tool,
                action,
                context,
                assessment);

        createResult.IsSuccess.Should().BeTrue();

        var pending =
            createResult.Value!;

        pending.ConfirmationId
            .Should()
            .NotBe(Guid.Empty);

        pending.ConfirmationToken
            .Should()
            .NotBeNullOrWhiteSpace();

        pending.ToolName
            .Should()
            .Be(tool.Name);

        pending.Version
            .Should()
            .Be(tool.Version);

        pending.UserId
            .Should()
            .Be(userId);

        pending.TenantId
            .Should()
            .Be(tenantId);

        pending.BranchId
            .Should()
            .Be(branchId);

        pending.ExpiresAtUtc
            .Should()
            .BeAfter(pending.CreatedAtUtc);

        var readResult =
            service.ReadToken(
                pending.ConfirmationToken);

        readResult.IsSuccess.Should().BeTrue();

        var restored =
            readResult.Value!;

        restored.ConfirmationId
            .Should()
            .Be(pending.ConfirmationId);

        restored.ToolName
            .Should()
            .Be(pending.ToolName);

        restored.Version
            .Should()
            .Be(pending.Version);

        restored.RiskLevel
            .Should()
            .Be(pending.RiskLevel);

        restored.UserId
            .Should()
            .Be(pending.UserId);

        restored.TenantId
            .Should()
            .Be(pending.TenantId);

        restored.BranchId
            .Should()
            .Be(pending.BranchId);

        restored.Arguments
            .Should()
            .ContainKey("admissionNumber");

        restored.Arguments["admissionNumber"]
            .GetString()
            .Should()
            .Be("ADM-001");
    }

    [Fact]
    public void Should_reject_tampered_token()
    {
        var service =
            CreateService();

        var context =
            CreateContext();

        var tool =
            CreateTool();

        var action =
            CreateAction(
                tool.Name,
                tool.Version);

        var assessment =
            CreateRiskAssessment(
                tool,
                requiresConfirmation: true);

        var createResult =
            service.CreatePendingAction(
                tool,
                action,
                context,
                assessment);

        var token =
            createResult.Value!.ConfirmationToken;

        var tamperedToken =
            token + "tampered";

        var result =
            service.ReadToken(
                tamperedToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Unauthorized");

        result.Error.Message
            .Should()
            .Be("Invalid or tampered AI confirmation token.");
    }

    [Fact]
    public void Should_reject_empty_token()
    {
        var service =
            CreateService();

        var result =
            service.ReadToken(
                string.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Unauthorized");
    }

    [Fact]
    public void Should_reject_unauthenticated_context()
    {
        var service =
            CreateService();

        var context =
            new AiExecutionContext
            {
                IsAuthenticated = false
            };

        var tool =
            CreateTool();

        var result =
            service.CreatePendingAction(
                tool,
                CreateAction(tool.Name, tool.Version),
                context,
                CreateRiskAssessment(
                    tool,
                    requiresConfirmation: true));

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Unauthorized");

        result.Error.Message
            .Should()
            .Contain("Authenticated user context");
    }

    [Fact]
    public void Should_reject_missing_user_id()
    {
        var service =
            CreateService();

        var context =
            new AiExecutionContext
            {
                IsAuthenticated = true,
                TenantId = Guid.NewGuid()
            };

        var tool =
            CreateTool();

        var result =
            service.CreatePendingAction(
                tool,
                CreateAction(tool.Name, tool.Version),
                context,
                CreateRiskAssessment(
                    tool,
                    requiresConfirmation: true));

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Unauthorized");
    }

    [Fact]
    public void Should_reject_missing_tenant_id()
    {
        var service =
            CreateService();

        var context =
            new AiExecutionContext
            {
                IsAuthenticated = true,
                UserId = Guid.NewGuid()
            };

        var tool =
            CreateTool();

        var result =
            service.CreatePendingAction(
                tool,
                CreateAction(tool.Name, tool.Version),
                context,
                CreateRiskAssessment(
                    tool,
                    requiresConfirmation: true));

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Unauthorized");

        result.Error.Message
            .Should()
            .Contain("Tenant context");
    }

    [Fact]
    public void Should_reject_when_confirmation_is_not_required()
    {
        var service =
            CreateService();

        var tool =
            CreateTool();

        var assessment =
            CreateRiskAssessment(
                tool,
                requiresConfirmation: false);

        var result =
            service.CreatePendingAction(
                tool,
                CreateAction(tool.Name, tool.Version),
                CreateContext(),
                assessment);

        result.IsFailure.Should().BeTrue();
        result.Error.Code
            .Should()
            .Be("Validation");

        result.Error.Message
            .Should()
            .Contain("can only be created");
    }

    [Fact]
    public void Should_reject_tool_action_name_mismatch()
    {
        var service =
            CreateService();

        var tool =
            CreateTool(
                name: "DeleteStudent");

        var action =
            CreateAction(
                "GetStudentByAdmissionNumber",
                tool.Version);

        var result =
            service.CreatePendingAction(
                tool,
                action,
                CreateContext(),
                CreateRiskAssessment(
                    tool,
                    requiresConfirmation: true));

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Contain("action name");
    }

    [Fact]
    public void Should_reject_invalid_token_lifetime_configuration()
    {
        var service =
            CreateService(
                tokenLifetimeSeconds: 0);

        var tool =
            CreateTool();

        var result =
            service.CreatePendingAction(
                tool,
                CreateAction(tool.Name, tool.Version),
                CreateContext(),
                CreateRiskAssessment(
                    tool,
                    requiresConfirmation: true));

        result.IsFailure.Should().BeTrue();
        result.Error.Message
            .Should()
            .Contain("token lifetime");
    }

    [Fact]
    public void Should_preserve_exact_action_arguments()
    {
        var service =
            CreateService();

        var tool =
            CreateTool();

        var action =
            new AiActionProposal
            {
                ActionName = tool.Name,
                Version = tool.Version,
                Arguments =
                    new Dictionary<string, JsonElement>
                    {
                        ["admissionNumber"] =
                            JsonSerializer.SerializeToElement(
                                "ADM-999"),

                        ["includeInactive"] =
                            JsonSerializer.SerializeToElement(
                                true)
                    }
            };

        var result =
            service.CreatePendingAction(
                tool,
                action,
                CreateContext(),
                CreateRiskAssessment(
                    tool,
                    requiresConfirmation: true));

        result.IsSuccess.Should().BeTrue();

        var restored =
            service.ReadToken(
                result.Value!.ConfirmationToken);

        restored.IsSuccess.Should().BeTrue();

        restored.Value!.Arguments[
                "admissionNumber"]
            .GetString()
            .Should()
            .Be("ADM-999");

        restored.Value!.Arguments[
                "includeInactive"]
            .GetBoolean()
            .Should()
            .BeTrue();
    }

    private static DataProtectionAiConfirmationTokenService
        CreateService(
            int tokenLifetimeSeconds = 300)
    {
        var provider =
            new EphemeralDataProtectionProvider();

        var options =
            Options.Create(
                new AiConfirmationOptions
                {
                    TokenLifetimeSeconds =
                        tokenLifetimeSeconds
                });

        return new DataProtectionAiConfirmationTokenService(
            provider,
            options);
    }

    private static AiExecutionContext CreateContext()
    {
        return new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Permissions =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
        };
    }

    private static ToolDefinition CreateTool(
        string name = "GetStudentByAdmissionNumber",
        int version = 1)
    {
        return new ToolDefinition
        {
            Name = name,
            Description = "Test tool.",
            Version = version,
            InputSchema =
                new System.Text.Json.Nodes.JsonObject
                {
                    ["type"] = "object"
                },
            RiskLevel = AiRiskLevel.High,
            ConfirmationPolicy =
                AiConfirmationPolicy.Required,
            AuditPolicy =
                AiAuditPolicy.Required
        };
    }

    private static AiActionProposal CreateAction(
        string actionName,
        int version)
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

    private static AiRiskAssessment CreateRiskAssessment(
        ToolDefinition tool,
        bool requiresConfirmation)
    {
        return new AiRiskAssessment
        {
            ToolName = tool.Name,
            Version = tool.Version,
            RiskLevel = tool.RiskLevel,
            RequiresConfirmation =
                requiresConfirmation,
            Reasons =
            [
                "Confirmation required for test."
            ]
        };
    }
}