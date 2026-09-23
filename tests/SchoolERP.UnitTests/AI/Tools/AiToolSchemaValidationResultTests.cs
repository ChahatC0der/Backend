using FluentAssertions;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.UnitTests.AI.Tools;

public sealed class AiToolSchemaValidationResultTests
{
    [Fact]
    public void Success_Should_Create_Valid_Result()
    {
        var result =
            AiToolSchemaValidationResult.Success();

        result.IsValid
            .Should()
            .BeTrue();

        result.Errors
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Failure_Should_Create_Invalid_Result_With_Errors()
    {
        var errors = new[]
        {
            "admissionNumber is required.",
            "Unknown property 'foo'."
        };

        var result =
            AiToolSchemaValidationResult.Failure(
                errors);

        result.IsValid
            .Should()
            .BeFalse();

        result.Errors
            .Should()
            .HaveCount(2);

        result.Errors
            .Should()
            .Contain(
                "admissionNumber is required.");

        result.Errors
            .Should()
            .Contain(
                "Unknown property 'foo'.");
    }

    [Fact]
    public void Failure_Should_Not_Expose_Mutable_Input()
    {
        var errors = new List<string>
        {
            "Invalid input."
        };

        var result =
            AiToolSchemaValidationResult.Failure(
                errors);

        errors.Add("Another error.");

        result.Errors
            .Should()
            .ContainSingle();
    }
}