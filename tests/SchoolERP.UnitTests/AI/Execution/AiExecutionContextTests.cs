using FluentAssertions;
using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.UnitTests.AI.Execution;

public sealed class AiExecutionContextTests
{
    [Fact]
    public void DefaultContext_ShouldBeUnauthenticated()
    {
        var context = new AiExecutionContext();

        context.IsAuthenticated.Should().BeFalse();
        context.UserId.Should().BeNull();
        context.TenantId.Should().BeNull();
        context.BranchId.Should().BeNull();
        context.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void Permissions_ShouldBeCaseInsensitive()
    {
        var context = new AiExecutionContext
        {
            IsAuthenticated = true,
            Permissions = new HashSet<string>(
                new[]
                {
                    "student.read"
                },
                StringComparer.OrdinalIgnoreCase)
        };

        context.Permissions
            .Contains("STUDENT.READ")
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Context_ShouldStoreSecurityInformation()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        var context = new AiExecutionContext
        {
            IsAuthenticated = true,
            UserId = userId,
            TenantId = tenantId,
            BranchId = branchId,
            Permissions = new HashSet<string>(
                new[]
                {
                    "student.read",
                    "student.update"
                },
                StringComparer.OrdinalIgnoreCase)
        };

        context.IsAuthenticated.Should().BeTrue();
        context.UserId.Should().Be(userId);
        context.TenantId.Should().Be(tenantId);
        context.BranchId.Should().Be(branchId);

        context.Permissions
            .Should()
            .Contain("student.read");

        context.Permissions
            .Should()
            .Contain("student.update");
    }
}