using FluentAssertions;
using System.Text.Json.Nodes;
using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Infrastructure.Services.AI.Tools;

namespace SchoolERP.UnitTests.AI.Tools;

public sealed class InMemoryAiToolRegistryTests
{
    [Fact]
    public void GetAll_Should_Return_Empty_When_No_Tools_Are_Registered()
    {
        var registry =
            new InMemoryAiToolRegistry();

        registry
            .GetAll()
            .Should()
            .BeEmpty();
    }

    [Fact]
    public void Register_Should_Make_Tool_Available()
    {
        var registry =
            new InMemoryAiToolRegistry();

        var tool = CreateTool(
            "GetStudentById",
            1);

        registry.Register(tool);

        var result = registry.Get(
            "GetStudentById",
            1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("GetStudentById");
        result.Version.Should().Be(1);
    }

    [Fact]
    public void Get_Should_Return_Null_For_Unknown_Tool()
    {
        var registry =
            new InMemoryAiToolRegistry();

        var result = registry.Get(
            "UnknownTool",
            1);

        result.Should().BeNull();
    }

    [Fact]
    public void Register_Should_Reject_Duplicate_Name_And_Version()
    {
        var registry =
            new InMemoryAiToolRegistry();

        var tool = CreateTool(
            "GetStudentById",
            1);

        registry.Register(tool);

        var act = () =>
            registry.Register(
                CreateTool(
                    "GetStudentById",
                    1));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(
                "AI tool 'GetStudentById' version '1' is already registered.");
    }

    [Fact]
    public void Register_Should_Allow_Different_Versions()
    {
        var registry =
            new InMemoryAiToolRegistry();

        registry.Register(
            CreateTool(
                "GetStudentById",
                1));

        registry.Register(
            CreateTool(
                "GetStudentById",
                2));

        registry.Get(
            "GetStudentById",
            1)
            .Should()
            .NotBeNull();

        registry.Get(
            "GetStudentById",
            2)
            .Should()
            .NotBeNull();

        registry.GetAll()
            .Should()
            .HaveCount(2);
    }

    [Fact]
    public void Register_Should_Reject_Empty_Name()
    {
        var registry =
            new InMemoryAiToolRegistry();

        var act = () =>
            registry.Register(
                CreateTool(
                    string.Empty,
                    1));

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*Tool name is required.*");
    }

    [Fact]
    public void Register_Should_Reject_Invalid_Version()
    {
        var registry =
            new InMemoryAiToolRegistry();

        var act = () =>
            registry.Register(
                CreateTool(
                    "GetStudentById",
                    0));

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage(
                "*Tool version must be greater than zero.*");
    }

    [Fact]
    public void Get_Should_Be_Case_Insensitive()
    {
        var registry =
            new InMemoryAiToolRegistry();

        registry.Register(
            CreateTool(
                "GetStudentById",
                1));

        var result = registry.Get(
            "getstudentbyid",
            1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("GetStudentById");
    }

    private static ToolDefinition CreateTool(
        string name,
        int version)
    {
        return new ToolDefinition
        {
            Name = name,

            Description =
                "Test tool.",

            Version = version,

            InputSchema = new JsonObject
            {
                ["type"] = "object"
            }
        };
    }
}