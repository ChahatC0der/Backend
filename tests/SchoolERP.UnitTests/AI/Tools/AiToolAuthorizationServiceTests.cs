using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Moq;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Application.Features.AI.Tools;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Services;

public sealed class AiToolAuthorizationServiceTests
{
    private readonly Mock<IAiExecutionContextAccessor>
        _contextAccessorMock;

    public AiToolAuthorizationServiceTests()
    {
        _contextAccessorMock =
            new Mock<IAiExecutionContextAccessor>();
    }

    [Fact]
    public void Authorize_NullTool_Denies()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = sut.Authorize(null!);

        // Assert
        Assert.False(result.IsAllowed);

        Assert.Contains(
            "Tool definition is required.",
            result.Errors);
    }

    [Fact]
    public void Authorize_UnauthenticatedUser_Denies()
    {
        // Arrange
        var context = new AiExecutionContext
        {
            IsAuthenticated = false
        };

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool();

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.False(result.IsAllowed);

        Assert.Contains(
            "Authentication is required to use AI tools.",
            result.Errors);
    }

    [Fact]
    public void Authorize_AuthenticatedUserWithoutRequiredPermission_Allows()
    {
        // Arrange
        var context = CreateAuthenticatedContext();

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(
            requiredPermission: null);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Authorize_RequiredPermissionPresent_Allows()
    {
        // Arrange
        var context = CreateAuthenticatedContext(
            "student.read",
            "student.update");

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(
            requiredPermission: "student.read");

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.True(result.IsAllowed);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Authorize_RequiredPermissionMissing_Denies()
    {
        // Arrange
        var context = CreateAuthenticatedContext(
            "student.read");

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(
            requiredPermission: "student.update");

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.False(result.IsAllowed);

        Assert.Contains(
            "Permission 'student.update' is required.",
            result.Errors);
    }

    [Fact]
    public void Authorize_PermissionCheckIsCaseInsensitive()
    {
        // Arrange
        var context = CreateAuthenticatedContext(
            "Student.Read");

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(
            requiredPermission: "student.read");

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Authorize_TenantScopedTool_WithTenantId_Allows()
    {
        // Arrange
        var context = CreateAuthenticatedContext(
            tenantId: Guid.NewGuid());

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(
            dataScope: AiDataScope.Tenant);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Authorize_TenantScopedTool_WithoutTenantId_Denies()
    {
        // Arrange
        var context = CreateAuthenticatedContext(
            tenantId: null);

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(
            dataScope: AiDataScope.Tenant);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.False(result.IsAllowed);

        Assert.Contains(
            "Current tenant context is required for tenant-scoped tools.",
            result.Errors);
    }

    [Fact]
    public void Authorize_BranchScopedTool_WithTenantAndBranch_Allows()
    {
        // Arrange
        var context = new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Permissions = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
        };

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(
            dataScope: AiDataScope.Branch);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Authorize_BranchScopedTool_WithoutBranchId_Denies()
    {
        // Arrange
        var context = new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BranchId = null,
            Permissions = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
        };

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(
            dataScope: AiDataScope.Branch);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.False(result.IsAllowed);

        Assert.Contains(
            "Current branch context is required for branch-scoped tools.",
            result.Errors);
    }

    [Fact]
    public void Authorize_UserScopedTool_WithUserId_Allows()
    {
        // Arrange
        var context = CreateAuthenticatedContext(
            userId: Guid.NewGuid());

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(
            dataScope: AiDataScope.User);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Authorize_UserScopedTool_WithoutUserId_Denies()
    {
        // Arrange
        var context = new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = null,
            TenantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Permissions = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
        };

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(
            dataScope: AiDataScope.User);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.False(result.IsAllowed);

        Assert.Contains(
            "Current user context is required for this tool.",
            result.Errors);
    }

    [Fact]
    public void Authorize_PermissionFailure_DoesNotContinueToScopeAuthorization()
    {
        // Arrange
        var context = CreateAuthenticatedContext(
            "student.read");

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(
            requiredPermission: "student.update",
            dataScope: AiDataScope.Tenant);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.False(result.IsAllowed);

        Assert.Contains(
            "Permission 'student.update' is required.",
            result.Errors);
    }

    private AiToolAuthorizationService CreateSut()
    {
        var scopeAuthorizationService =
            new AiToolScopeAuthorizationService(
                _contextAccessorMock.Object);

        return new AiToolAuthorizationService(
            _contextAccessorMock.Object,
            scopeAuthorizationService);
    }

    private static ToolDefinition CreateTool(
        string? requiredPermission = null,
        AiDataScope dataScope = AiDataScope.None)
    {
        return new ToolDefinition
        {
            Name = "TestTool",
            Description = "Test AI tool",
            Version = 1,
            InputSchema = new JsonObject(),
            RequiredPermission = requiredPermission,
            DataScope = dataScope,
            RiskLevel = AiRiskLevel.Low,
            ConfirmationPolicy = AiConfirmationPolicy.None,
            AuditPolicy = AiAuditPolicy.Required
        };
    }

    private static AiExecutionContext CreateAuthenticatedContext(
        params string[] permissions)
    {
        return new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Permissions = new HashSet<string>(
                permissions,
                StringComparer.OrdinalIgnoreCase)
        };
    }

    private static AiExecutionContext CreateAuthenticatedContext(
        Guid? userId = null,
        Guid? tenantId = null)
    {
        return new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = userId,
            TenantId = tenantId,
            BranchId = Guid.NewGuid(),
            Permissions = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
        };
    }
}