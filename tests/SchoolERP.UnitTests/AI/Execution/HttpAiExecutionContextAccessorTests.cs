using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Infrastructure.Services.AI;
using Xunit;

namespace SchoolERP.UnitTests.Infrastructure.Services.AI;

public sealed class HttpAiExecutionContextAccessorTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ICurrentTenantService> _currentTenantServiceMock;
    private readonly Mock<IAiPermissionProvider> _permissionProviderMock;
    private readonly Mock<IAiBranchContextProvider> _branchContextProviderMock;

    public HttpAiExecutionContextAccessorTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _currentTenantServiceMock = new Mock<ICurrentTenantService>();
        _permissionProviderMock = new Mock<IAiPermissionProvider>();
        _branchContextProviderMock = new Mock<IAiBranchContextProvider>();
    }

    [Fact]
    public void GetCurrent_WhenHttpContextIsNull_ReturnsDefaultContext()
    {
        // Arrange
        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns((HttpContext?)null);

        var sut = CreateSut();

        // Act
        var result = sut.GetCurrent();

        // Assert
        Assert.False(result.IsAuthenticated);
        Assert.Null(result.UserId);
        Assert.Null(result.TenantId);
        Assert.Null(result.BranchId);
        Assert.Empty(result.Permissions);

        _currentTenantServiceMock.Verify(
            x => x.GetTenantId(),
            Times.Never);

        _permissionProviderMock.Verify(
            x => x.GetPermissions(It.IsAny<ClaimsPrincipal>()),
            Times.Never);

        _branchContextProviderMock.Verify(
            x => x.GetBranchId(It.IsAny<ClaimsPrincipal>()),
            Times.Never);
    }

    [Fact]
    public void GetCurrent_WhenUserIsNotAuthenticated_ReturnsUnauthenticatedContext()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity())
        };

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(httpContext);

        var sut = CreateSut();

        // Act
        var result = sut.GetCurrent();

        // Assert
        Assert.False(result.IsAuthenticated);
        Assert.Null(result.UserId);
        Assert.Null(result.TenantId);
        Assert.Null(result.BranchId);
        Assert.Empty(result.Permissions);

        _currentTenantServiceMock.Verify(
            x => x.GetTenantId(),
            Times.Never);

        _permissionProviderMock.Verify(
            x => x.GetPermissions(It.IsAny<ClaimsPrincipal>()),
            Times.Never);

        _branchContextProviderMock.Verify(
            x => x.GetBranchId(It.IsAny<ClaimsPrincipal>()),
            Times.Never);
    }

    [Fact]
    public void GetCurrent_WhenAuthenticated_MapsUserTenantBranchAndPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        var permissions = new HashSet<string>(
            new[]
            {
                "student.read",
                "student.update"
            },
            StringComparer.OrdinalIgnoreCase);

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                userId.ToString())
        };

        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "Test");

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(httpContext);

        _currentTenantServiceMock
            .Setup(x => x.GetTenantId())
            .Returns(tenantId);

        _branchContextProviderMock
            .Setup(x => x.GetBranchId(principal))
            .Returns(branchId);

        _permissionProviderMock
            .Setup(x => x.GetPermissions(principal))
            .Returns(permissions);

        var sut = CreateSut();

        // Act
        var result = sut.GetCurrent();

        // Assert
        Assert.True(result.IsAuthenticated);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal(branchId, result.BranchId);

        Assert.NotNull(result.Permissions);
        Assert.Contains("student.read", result.Permissions);
        Assert.Contains("student.update", result.Permissions);

        _currentTenantServiceMock.Verify(
            x => x.GetTenantId(),
            Times.Once);

        _branchContextProviderMock.Verify(
            x => x.GetBranchId(principal),
            Times.Once);

        _permissionProviderMock.Verify(
            x => x.GetPermissions(principal),
            Times.Once);
    }

    [Fact]
    public void GetCurrent_WhenBranchProviderReturnsNull_KeepsBranchIdNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                userId.ToString())
        };

        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "Test");

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(httpContext);

        _currentTenantServiceMock
            .Setup(x => x.GetTenantId())
            .Returns(tenantId);

        _branchContextProviderMock
            .Setup(x => x.GetBranchId(principal))
            .Returns((Guid?)null);

        _permissionProviderMock
            .Setup(x => x.GetPermissions(principal))
            .Returns(new HashSet<string>(
                StringComparer.OrdinalIgnoreCase));

        var sut = CreateSut();

        // Act
        var result = sut.GetCurrent();

        // Assert
        Assert.True(result.IsAuthenticated);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Null(result.BranchId);
    }

    [Fact]
    public void GetCurrent_WhenNameIdentifierIsMissing_ReturnsNullUserId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        var identity = new ClaimsIdentity(
            authenticationType: "Test");

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(httpContext);

        _currentTenantServiceMock
            .Setup(x => x.GetTenantId())
            .Returns(tenantId);

        _branchContextProviderMock
            .Setup(x => x.GetBranchId(principal))
            .Returns(branchId);

        _permissionProviderMock
            .Setup(x => x.GetPermissions(principal))
            .Returns(new HashSet<string>(
                StringComparer.OrdinalIgnoreCase));

        var sut = CreateSut();

        // Act
        var result = sut.GetCurrent();

        // Assert
        Assert.True(result.IsAuthenticated);
        Assert.Null(result.UserId);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal(branchId, result.BranchId);
    }

    [Fact]
    public void GetCurrent_WhenNameIdentifierIsInvalidGuid_ReturnsNullUserId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                "not-a-guid")
        };

        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "Test");

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(httpContext);

        _currentTenantServiceMock
            .Setup(x => x.GetTenantId())
            .Returns(tenantId);

        _branchContextProviderMock
            .Setup(x => x.GetBranchId(principal))
            .Returns(branchId);

        _permissionProviderMock
            .Setup(x => x.GetPermissions(principal))
            .Returns(new HashSet<string>(
                StringComparer.OrdinalIgnoreCase));

        var sut = CreateSut();

        // Act
        var result = sut.GetCurrent();

        // Assert
        Assert.True(result.IsAuthenticated);
        Assert.Null(result.UserId);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal(branchId, result.BranchId);
    }

    [Fact]
    public void GetCurrent_PreservesPermissionProviderSet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        var permissions = new HashSet<string>(
            new[]
            {
                "student.read",
                "student.create",
                "fee.read"
            },
            StringComparer.OrdinalIgnoreCase);

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                userId.ToString())
        };

        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "Test");

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(httpContext);

        _currentTenantServiceMock
            .Setup(x => x.GetTenantId())
            .Returns(tenantId);

        _branchContextProviderMock
            .Setup(x => x.GetBranchId(principal))
            .Returns(branchId);

        _permissionProviderMock
            .Setup(x => x.GetPermissions(principal))
            .Returns(permissions);

        var sut = CreateSut();

        // Act
        var result = sut.GetCurrent();

        // Assert
        Assert.Same(permissions, result.Permissions);
        Assert.Equal(branchId, result.BranchId);
    }

    private HttpAiExecutionContextAccessor CreateSut()
    {
        return new HttpAiExecutionContextAccessor(
            _httpContextAccessorMock.Object,
            _currentTenantServiceMock.Object,
            _permissionProviderMock.Object,
            _branchContextProviderMock.Object);
    }
}