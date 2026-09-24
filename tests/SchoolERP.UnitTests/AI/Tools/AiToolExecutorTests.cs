using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Domain.Shared.Results;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Tools;

public sealed class AiToolExecutorTests
{
    private readonly Mock<IAiExecutionContextAccessor>
        _contextAccessorMock;

    public AiToolExecutorTests()
    {
        _contextAccessorMock =
            new Mock<IAiExecutionContextAccessor>();
    }

    [Fact]
    public async Task ExecuteAsync_NullTool_DeniesWithoutCallingHandler()
    {
        // Arrange
        var handlerMock = CreateHandlerMock();

        var sut = CreateSut(handlerMock.Object);

        var action = CreateAction();

        // Act
        var result = await sut.ExecuteAsync(
            null!,
            action);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Validation",
            result.Error.Code);

        handlerMock.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_NullAction_DeniesWithoutCallingHandler()
    {
        // Arrange
        var handlerMock = CreateHandlerMock();

        var tool = CreateTool();

        var sut = CreateSut(handlerMock.Object);

        // Act
        var result = await sut.ExecuteAsync(
            tool,
            null!);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Validation",
            result.Error.Code);

        handlerMock.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_NameMismatch_DeniesWithoutCallingHandler()
    {
        // Arrange
        var handlerMock = CreateHandlerMock();

        var tool = CreateTool(
            name: "GetStudent");

        var action = CreateAction(
            actionName: "DeleteStudent");

        var sut = CreateSut(handlerMock.Object);

        // Act
        var result = await sut.ExecuteAsync(
            tool,
            action);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Validation",
            result.Error.Code);

        Assert.Contains(
            "Tool definition and action name do not match.",
            result.Error.Message);

        handlerMock.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_VersionMismatch_DeniesWithoutCallingHandler()
    {
        // Arrange
        var handlerMock = CreateHandlerMock();

        var tool = CreateTool(
            version: 1);

        var action = CreateAction(
            version: 2);

        var sut = CreateSut(handlerMock.Object);

        // Act
        var result = await sut.ExecuteAsync(
            tool,
            action);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Validation",
            result.Error.Code);

        Assert.Contains(
            "Tool definition and action version do not match.",
            result.Error.Message);

        handlerMock.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UnauthenticatedUser_DeniesWithoutCallingHandler()
    {
        // Arrange
        var handlerMock = CreateHandlerMock();

        SetupContext(new AiExecutionContext
        {
            IsAuthenticated = false
        });

        var tool = CreateTool();

        var action = CreateAction();

        var sut = CreateSut(handlerMock.Object);

        // Act
        var result = await sut.ExecuteAsync(
            tool,
            action);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Unauthorized",
            result.Error.Code);

        Assert.Contains(
            "Authentication is required to use AI tools.",
            result.Error.Message);

        handlerMock.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_MissingPermission_DeniesWithoutCallingHandler()
    {
        // Arrange
        var handlerMock = CreateHandlerMock();

        SetupContext(new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Permissions = new HashSet<string>(
                new[]
                {
                    "student.read"
                },
                StringComparer.OrdinalIgnoreCase)
        });

        var tool = CreateTool(
            requiredPermission: "student.update");

        var action = CreateAction();

        var sut = CreateSut(handlerMock.Object);

        // Act
        var result = await sut.ExecuteAsync(
            tool,
            action);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Unauthorized",
            result.Error.Code);

        Assert.Contains(
            "Permission 'student.update' is required.",
            result.Error.Message);

        handlerMock.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_MissingTenantScope_DeniesWithoutCallingHandler()
    {
        // Arrange
        var handlerMock = CreateHandlerMock();

        SetupContext(new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = null,
            BranchId = null,
            Permissions = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
        });

        var tool = CreateTool(
            dataScope: AiDataScope.Tenant);

        var action = CreateAction();

        var sut = CreateSut(handlerMock.Object);

        // Act
        var result = await sut.ExecuteAsync(
            tool,
            action);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Unauthorized",
            result.Error.Code);

        Assert.Contains(
            "Current tenant context is required for tenant-scoped tools.",
            result.Error.Message);

        handlerMock.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_MissingBranchScope_DeniesWithoutCallingHandler()
    {
        // Arrange
        var handlerMock = CreateHandlerMock();

        SetupContext(new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BranchId = null,
            Permissions = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
        });

        var tool = CreateTool(
            dataScope: AiDataScope.Branch);

        var action = CreateAction();

        var sut = CreateSut(handlerMock.Object);

        // Act
        var result = await sut.ExecuteAsync(
            tool,
            action);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Unauthorized",
            result.Error.Code);

        Assert.Contains(
            "Current branch context is required for branch-scoped tools.",
            result.Error.Message);

        handlerMock.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_MissingUserScope_DeniesWithoutCallingHandler()
    {
        // Arrange
        var handlerMock = CreateHandlerMock();

        SetupContext(new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = null,
            TenantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Permissions = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
        });

        var tool = CreateTool(
            dataScope: AiDataScope.User);

        var action = CreateAction();

        var sut = CreateSut(handlerMock.Object);

        // Act
        var result = await sut.ExecuteAsync(
            tool,
            action);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Unauthorized",
            result.Error.Code);

        Assert.Contains(
            "Current user context is required for this tool.",
            result.Error.Message);

        handlerMock.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidAuthorization_CallsMatchingHandler()
    {
        // Arrange
        var handlerMock = CreateHandlerMock();

        SetupContext(new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Permissions = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
        });

        var arguments =
            new Dictionary<string, JsonElement>
            {
                ["admissionNumber"] =
                    JsonSerializer.SerializeToElement("ADM-001")
            };

        handlerMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Success(
                    new AiToolExecutionResult
                    {
                        ToolName = "GetStudent",
                        Output = new
                        {
                            Id = 1
                        },
                        Message = "Student found."
                    }));

        var tool = CreateTool(
            name: "GetStudent");

        var action = CreateAction(
            actionName: "GetStudent",
            arguments: arguments);

        var sut = CreateSut(handlerMock.Object);

        // Act
        var result = await sut.ExecuteAsync(
            tool,
            action);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(
            "GetStudent",
            result.Value!.ToolName);

        handlerMock.Verify(
            x => x.ExecuteAsync(
                It.Is<IReadOnlyDictionary<string, JsonElement>>(
                    args =>
                        args.ContainsKey("admissionNumber")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NoMatchingHandler_ReturnsValidationFailure()
    {
        // Arrange
        var handlerMock = CreateHandlerMock();

        SetupContext(CreateAuthenticatedContext());

        var tool = CreateTool(
            name: "GetStudent");

        var action = CreateAction(
            actionName: "GetStudent");

        var sut = CreateSut(handlerMock.Object);

        // Act
        var result = await sut.ExecuteAsync(
            tool,
            action);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Validation",
            result.Error.Code);

        Assert.Contains(
            "No executor is registered for tool 'GetStudent' version 1.",
            result.Error.Message);

        handlerMock.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_HandlerFailure_ReturnsHandlerFailure()
    {
        // Arrange
        var handlerMock = CreateHandlerMock();

        SetupContext(CreateAuthenticatedContext());

        handlerMock
            .Setup(x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result.Failure<AiToolExecutionResult>(
                    Error.Validation(
                        "Tool execution failed.")));

        var tool = CreateTool(
            name: "GetStudent");

        var action = CreateAction(
            actionName: "GetStudent");

        var sut = CreateSut(handlerMock.Object);

        // Act
        var result = await sut.ExecuteAsync(
            tool,
            action);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            "Validation",
            result.Error.Code);

        Assert.Equal(
            "Tool execution failed.",
            result.Error.Message);

        handlerMock.Verify(
            x => x.ExecuteAsync(
                It.IsAny<IReadOnlyDictionary<string, JsonElement>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private AiToolExecutor CreateSut(
        IAiToolHandler handler)
    {
        var authorizationService =
            new AiToolAuthorizationService(
                _contextAccessorMock.Object,
                new AiToolScopeAuthorizationService(
                    _contextAccessorMock.Object));

        return new AiToolExecutor(
            new[] { handler },
            authorizationService);
    }

    private void SetupContext(
        AiExecutionContext context)
    {
        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);
    }

    private static Mock<IAiToolHandler> CreateHandlerMock()
    {
        var mock = new Mock<IAiToolHandler>();

        mock.SetupGet(x => x.Name)
            .Returns("GetStudent");

        mock.SetupGet(x => x.Version)
            .Returns(1);

        return mock;
    }

    private static AiExecutionContext CreateAuthenticatedContext()
    {
        return new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Permissions = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
        };
    }

    private static ToolDefinition CreateTool(
        string name = "GetStudent",
        int version = 1,
        string? requiredPermission = null,
        AiDataScope dataScope = AiDataScope.None)
    {
        return new ToolDefinition
        {
            Name = name,
            Description = "Test AI tool.",
            Version = version,
            InputSchema = new JsonObject(),
            RequiredPermission = requiredPermission,
            DataScope = dataScope,
            RiskLevel = AiRiskLevel.Low,
            ConfirmationPolicy = AiConfirmationPolicy.None,
            AuditPolicy = AiAuditPolicy.Required
        };
    }

    private static AiActionProposal CreateAction(
        string actionName = "GetStudent",
        int version = 1,
        IReadOnlyDictionary<string, JsonElement>? arguments = null)
    {
        return new AiActionProposal
        {
            ActionName = actionName,
            Version = version,
            Arguments =
                arguments ??
                new Dictionary<string, JsonElement>()
        };
    }
}