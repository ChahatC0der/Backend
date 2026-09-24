using System;
using Moq;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Application.Features.AI.Tools;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Services;

public sealed class AiToolScopeAuthorizationServiceTests
{
    private readonly Mock<IAiExecutionContextAccessor>
        _contextAccessorMock;

    public AiToolScopeAuthorizationServiceTests()
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
    public void Authorize_NoneScope_AllowsAuthenticatedContext()
    {
        // Arrange
        var context = CreateAuthenticatedContext();

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(AiDataScope.None);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Authorize_UserScope_WithUserId_Allows()
    {
        // Arrange
        var context = CreateAuthenticatedContext();

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(AiDataScope.User);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Authorize_UserScope_WithoutUserId_Denies()
    {
        // Arrange
        var context = new AiExecutionContext
        {
            IsAuthenticated = true,
            TenantId = Guid.NewGuid(),
            UserId = null
        };

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(AiDataScope.User);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.False(result.IsAllowed);

        Assert.Contains(
            "Current user context is required",
            result.Errors);
    }

    [Fact]
    public void Authorize_TenantScope_WithTenantId_Allows()
    {
        // Arrange
        var context = CreateAuthenticatedContext();

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(AiDataScope.Tenant);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Authorize_TenantScope_WithoutTenantId_Denies()
    {
        // Arrange
        var context = new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = null
        };

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(AiDataScope.Tenant);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.False(result.IsAllowed);

        Assert.Contains(
            "Current tenant context is required",
            result.Errors);
    }

    [Fact]
    public void Authorize_BranchScope_WithTenantAndBranch_Allows()
    {
        // Arrange
        var context = new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid()
        };

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(AiDataScope.Branch);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Authorize_BranchScope_WithoutTenantId_Denies()
    {
        // Arrange
        var context = new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = null,
            BranchId = Guid.NewGuid()
        };

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(AiDataScope.Branch);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.False(result.IsAllowed);

        Assert.Contains(
            "Current tenant context is required",
            result.Errors);
    }

    [Fact]
    public void Authorize_BranchScope_WithoutBranchId_Denies()
    {
        // Arrange
        var context = new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BranchId = null
        };

        _contextAccessorMock
            .Setup(x => x.GetCurrent())
            .Returns(context);

        var tool = CreateTool(AiDataScope.Branch);

        var sut = CreateSut();

        // Act
        var result = sut.Authorize(tool);

        // Assert
        Assert.False(result.IsAllowed);

        Assert.Contains(
            "Current branch context is required",
            result.Errors);
    }

    private AiToolScopeAuthorizationService CreateSut()
    {
        return new AiToolScopeAuthorizationService(
            _contextAccessorMock.Object);
    }

    private static AiToolExecutionResult DummyResult()
        => new()
        {
            ToolName = "TestTool"
        };

    private static AiExecutionContext CreateAuthenticatedContext()
    {
        return new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid()
        };
    }

    private static ToolDefinition CreateTool(
        AiDataScope dataScope)
    {
        return new ToolDefinition
        {
            Name = "TestTool",
            Description = "Test AI tool",
            InputSchema = new System.Text.Json.Nodes.JsonObject(),
            DataScope = dataScope
        };
    }
}