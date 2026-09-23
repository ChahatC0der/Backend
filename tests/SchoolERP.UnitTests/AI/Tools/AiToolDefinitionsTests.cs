using FluentAssertions;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.UnitTests.AI.Tools;

public sealed class AiToolDefinitionsTests
{
    [Fact]
    public void GetAll_Should_Contain_GetStudentByAdmissionNumber()
    {
        var tools =
            AiToolDefinitions.GetAll();

        var tool = tools.SingleOrDefault(
            x => x.Name ==
                "GetStudentByAdmissionNumber"
            && x.Version == 1);

        tool.Should().NotBeNull();

        tool!.Description
            .Should()
            .NotBeNullOrWhiteSpace();

        tool.InputSchema["type"]!
            .GetValue<string>()
            .Should()
            .Be("object");

        tool.InputSchema["properties"]!
            .AsObject()
            .Should();
            //.ContainKey("admissionNumber");
    }
}