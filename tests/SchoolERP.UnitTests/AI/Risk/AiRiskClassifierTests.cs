using FluentAssertions;
using SchoolERP.Application.Features.AI.Risk;
using SchoolERP.Application.Features.AI.Tools;
using Xunit;

namespace SchoolERP.UnitTests.Features.AI.Risk;

public sealed class AiRiskClassifierTests
{
    private readonly AiRiskClassifier _sut;

    public AiRiskClassifierTests()
    {
        _sut = new AiRiskClassifier();
    }

    [Fact]
    public void Classify_ShouldFail_WhenToolIsNull()
    {
        // Act
        var result =
            _sut.Classify(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        result.Error.Message
            .Should()
            .Be("Tool definition is required.");
    }

    [Fact]
    public void Classify_ShouldFail_WhenToolNameIsEmpty()
    {
        // Arrange
        var tool = CreateTool(
            name: "");

        // Act
        var result =
            _sut.Classify(tool);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        result.Error.Message
            .Should()
            .Be("Tool name is required.");
    }

    [Fact]
    public void Classify_ShouldFail_WhenToolVersionIsInvalid()
    {
        // Arrange
        var tool =
            CreateTool(version: 0);

        // Act
        var result =
            _sut.Classify(tool);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        result.Error.Message
            .Should()
            .Be("Tool version must be greater than zero.");
    }

    [Fact]
    public void Classify_ShouldAllow_LowRiskWithoutConfirmation()
    {
        // Arrange
        var tool =
            CreateTool(
                riskLevel: AiRiskLevel.Low,
                confirmationPolicy: AiConfirmationPolicy.None);

        // Act
        var result =
            _sut.Classify(tool);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RiskLevel
            .Should()
            .Be(AiRiskLevel.Low);

        result.Value.RequiresConfirmation
            .Should()
            .BeFalse();

        result.Value.CanExecuteImmediately
            .Should()
            .BeTrue();

        result.Value.Reasons
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Classify_ShouldRequireConfirmation_WhenLowRiskToolRequiresIt()
    {
        // Arrange
        var tool =
            CreateTool(
                riskLevel: AiRiskLevel.Low,
                confirmationPolicy: AiConfirmationPolicy.Required);

        // Act
        var result =
            _sut.Classify(tool);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value!.RiskLevel
            .Should()
            .Be(AiRiskLevel.Low);

        result.Value.RequiresConfirmation
            .Should()
            .BeTrue();

        result.Value.CanExecuteImmediately
            .Should()
            .BeFalse();

        result.Value.Reasons
            .Should()
            .Contain(
                "Tool confirmation policy requires explicit confirmation.");
    }

    [Fact]
    public void Classify_ShouldAllow_MediumRiskWithoutConfirmation()
    {
        // Arrange
        var tool =
            CreateTool(
                riskLevel: AiRiskLevel.Medium,
                confirmationPolicy: AiConfirmationPolicy.None);

        // Act
        var result =
            _sut.Classify(tool);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value!.RiskLevel
            .Should()
            .Be(AiRiskLevel.Medium);

        result.Value.RequiresConfirmation
            .Should()
            .BeFalse();

        result.Value.CanExecuteImmediately
            .Should()
            .BeTrue();

        result.Value.Reasons
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Classify_ShouldRequireConfirmation_WhenMediumRiskToolRequiresIt()
    {
        // Arrange
        var tool =
            CreateTool(
                riskLevel: AiRiskLevel.Medium,
                confirmationPolicy: AiConfirmationPolicy.Required);

        // Act
        var result =
            _sut.Classify(tool);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value!.RequiresConfirmation
            .Should()
            .BeTrue();

        result.Value.CanExecuteImmediately
            .Should()
            .BeFalse();

        result.Value.Reasons
            .Should()
            .Contain(
                "Tool confirmation policy requires explicit confirmation.");
    }

    [Fact]
    public void Classify_ShouldAlwaysRequireConfirmation_ForHighRiskTool()
    {
        // Arrange
        var tool =
            CreateTool(
                riskLevel: AiRiskLevel.High,
                confirmationPolicy: AiConfirmationPolicy.None);

        // Act
        var result =
            _sut.Classify(tool);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value!.RiskLevel
            .Should()
            .Be(AiRiskLevel.High);

        result.Value.RequiresConfirmation
            .Should()
            .BeTrue();

        result.Value.CanExecuteImmediately
            .Should()
            .BeFalse();

        result.Value.Reasons
            .Should()
            .Contain(
                "High-risk AI actions require explicit confirmation.");
    }

    [Fact]
    public void Classify_ShouldAlwaysRequireConfirmation_ForCriticalRiskTool()
    {
        // Arrange
        var tool =
            CreateTool(
                riskLevel: AiRiskLevel.Critical,
                confirmationPolicy: AiConfirmationPolicy.None);

        // Act
        var result =
            _sut.Classify(tool);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value!.RiskLevel
            .Should()
            .Be(AiRiskLevel.Critical);

        result.Value.RequiresConfirmation
            .Should()
            .BeTrue();

        result.Value.CanExecuteImmediately
            .Should()
            .BeFalse();

        result.Value.Reasons
            .Should()
            .Contain(
                "Critical-risk AI actions always require explicit confirmation.");
    }

    [Fact]
    public void Classify_ShouldRequireConfirmation_ForHighRiskEvenWhenPolicySaysNone()
    {
        // Arrange
        var tool =
            CreateTool(
                riskLevel: AiRiskLevel.High,
                confirmationPolicy: AiConfirmationPolicy.None);

        // Act
        var result =
            _sut.Classify(tool);

        // Assert
        result.Value!.RequiresConfirmation
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Classify_ShouldRequireConfirmation_ForCriticalRiskEvenWhenPolicySaysNone()
    {
        // Arrange
        var tool =
            CreateTool(
                riskLevel: AiRiskLevel.Critical,
                confirmationPolicy: AiConfirmationPolicy.None);

        // Act
        var result =
            _sut.Classify(tool);

        // Assert
        result.Value!.RequiresConfirmation
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Classify_ShouldPreserveToolName()
    {
        // Arrange
        var tool =
            CreateTool(
                name: "CreateStudent");

        // Act
        var result =
            _sut.Classify(tool);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.ToolName
            .Should()
            .Be("CreateStudent");
    }

    [Fact]
    public void Classify_ShouldPreserveToolVersion()
    {
        // Arrange
        var tool =
            CreateTool(
                version: 3);

        // Act
        var result =
            _sut.Classify(tool);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Version
            .Should()
            .Be(3);
    }

    [Fact]
    public void Classify_ShouldNotTakeRiskLevelFromAction()
    {
        // This test documents the architectural rule:
        // risk is derived from ToolDefinition metadata,
        // not from AiActionProposal.

        var tool =
            CreateTool(
                riskLevel: AiRiskLevel.Critical);

        var result =
            _sut.Classify(tool);

        result.IsSuccess.Should().BeTrue();
        result.Value!.RiskLevel
            .Should()
            .Be(AiRiskLevel.Critical);
    }

    [Fact]
    public void Classify_ShouldFailClosed_ForUnsupportedRiskLevel()
    {
        // Arrange
        var tool =
            CreateTool();

        tool = tool with
        {
            RiskLevel = (AiRiskLevel)999
        };

        // Act
        var result =
            _sut.Classify(tool);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        result.Error.Message
            .Should()
            .Contain(
                "Unsupported AI risk level");
    }

    private static ToolDefinition CreateTool(
        string name = "TestTool",
        int version = 1,
        AiRiskLevel riskLevel = AiRiskLevel.Low,
        AiConfirmationPolicy confirmationPolicy =
            AiConfirmationPolicy.None)
    {
        return new ToolDefinition
        {
            Name = name,

            Description =
                "Test AI tool.",

            Version = version,

            InputSchema =
                new System.Text.Json.Nodes.JsonObject(),

            RequiredPermission = null,

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
}