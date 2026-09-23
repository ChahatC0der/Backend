using FluentAssertions;
using SchoolERP.Application.Features.AI;
using SchoolERP.Application.Features.AI.DTOs;
using System.Text.Json;

namespace SchoolERP.UnitTests.AI;

public sealed class AiActionProposalTests
{
    [Fact]
    public void Should_Create_Semantic_Action_Proposal()
    {
        var arguments = new Dictionary<string, JsonElement>
        {
            ["admissionNumber"] =
                JsonSerializer.SerializeToElement("ADM-1001")
        };

        var action = new AiActionProposal
        {
            ActionName = "GetStudentByAdmissionNumber",
            Arguments = arguments
        };

        action.ActionName
            .Should()
            .Be("GetStudentByAdmissionNumber");

        action.Arguments
            .Should()
            .ContainKey("admissionNumber");

        action.Arguments["admissionNumber"]
            .GetString()
            .Should()
            .Be("ADM-1001");
    }

    [Fact]
    public void Should_Distinguish_Action_Response_From_Message_Response()
    {
        var action = new AiActionProposal
        {
            ActionName = "GetStudentByAdmissionNumber"
        };

        var response = new AiSemanticResponse
        {
            Kind = AiResponseKind.Action,
            Message = "Fetching student details.",
            ProposedAction = action
        };

        response.Kind.Should().Be(AiResponseKind.Action);
        response.ProposedAction.Should().NotBeNull();
        response.ProposedAction!.ActionName
            .Should()
            .Be("GetStudentByAdmissionNumber");
    }

    [Fact]
    public void Should_Support_Normal_Message_Response()
    {
        var response = new AiSemanticResponse
        {
            Kind = AiResponseKind.Message,
            Message = "Hello SchoolERP"
        };

        response.Kind.Should().Be(AiResponseKind.Message);
        response.Message.Should().Be("Hello SchoolERP");
        response.ProposedAction.Should().BeNull();
    }
    [Fact]
    public void Should_Use_Canonical_Action_Name()
    {
        var action = new AiActionProposal
        {
            ActionName =
                AiActionNames.GetStudentByAdmissionNumber
        };

        action.ActionName
            .Should()
            .Be("GetStudentByAdmissionNumber");
    }

    [Fact]
    public void Should_Preserve_Action_Contract_Version()
    {
        var action = new AiActionProposal
        {
            ActionName = AiActionNames.GetStudentByAdmissionNumber,
            Version = 2
        };

        action.ActionName
            .Should()
            .Be(AiActionNames.GetStudentByAdmissionNumber);

        action.Version.Should().Be(2);
    }
}