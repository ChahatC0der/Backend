using FluentAssertions;
using SchoolERP.Application.Features.AI.Confirmation;
using SchoolERP.Application.Features.AI.Tools;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Confirmation;

public sealed class AiConfirmationRequirementTests
{
    [Fact]
    public void Should_store_confirmation_metadata()
    {
        var confirmationId =
            Guid.NewGuid();

        var expiresAt =
            DateTimeOffset.UtcNow.AddMinutes(5);

        var requirement =
            new AiConfirmationRequirement
            {
                ConfirmationId = confirmationId,
                ConfirmationToken = "protected-token",
                ToolName = "DeleteStudent",
                Version = 2,
                RiskLevel = AiRiskLevel.High,
                ExpiresAtUtc = expiresAt,
                Reasons =
                [
                    "High-risk operation.",
                    "This action changes persistent data."
                ]
            };

        requirement.ConfirmationId
            .Should()
            .Be(confirmationId);

        requirement.ConfirmationToken
            .Should()
            .Be("protected-token");

        requirement.ToolName
            .Should()
            .Be("DeleteStudent");

        requirement.Version
            .Should()
            .Be(2);

        requirement.RiskLevel
            .Should()
            .Be(AiRiskLevel.High);

        requirement.ExpiresAtUtc
            .Should()
            .Be(expiresAt);

        requirement.Reasons
            .Should()
            .HaveCount(2);
    }

    [Fact]
    public void Should_default_reasons_to_empty_collection()
    {
        var requirement =
            new AiConfirmationRequirement
            {
                ConfirmationId = Guid.NewGuid(),
                ConfirmationToken = "protected-token",
                ToolName = "CreateStudent",
                Version = 1,
                RiskLevel = AiRiskLevel.Medium,
                ExpiresAtUtc =
                    DateTimeOffset.UtcNow.AddMinutes(5)
            };

        requirement.Reasons
            .Should()
            .NotBeNull();

        requirement.Reasons
            .Should()
            .BeEmpty();
    }
}