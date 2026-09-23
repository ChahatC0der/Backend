using FluentAssertions;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Services;

namespace SchoolERP.UnitTests.AI;

public sealed class AiSemanticResponseParserTests
{
    [Fact]
    public void Should_Parse_Action_Response()
    {
        const string json =
            """
            {
              "kind": "Action",
              "message": "Fetching student details.",
              "proposedAction": {
                "actionName": "GetStudentByAdmissionNumber",
                "version": 1,
                "arguments": {
                  "admissionNumber": "ADM-1001"
                }
              }
            }
            """;

        var result =
            AiSemanticResponseParser.Parse(json);

        result.Kind
            .Should()
            .Be(AiResponseKind.Action);

        result.ProposedAction
            .Should()
            .NotBeNull();

        result.ProposedAction!.ActionName
            .Should()
            .Be("GetStudentByAdmissionNumber");

        result.ProposedAction.Version
            .Should()
            .Be(1);

        result.ProposedAction.Arguments
            .Should()
            .ContainKey("admissionNumber");
    }

    [Fact]
    public void Should_Parse_Message_Response()
    {
        const string json =
            """
            {
              "kind": "Message",
              "message": "Hello SchoolERP"
            }
            """;

        var result =
            AiSemanticResponseParser.Parse(json);

        result.Kind
            .Should()
            .Be(AiResponseKind.Message);

        result.Message
            .Should()
            .Be("Hello SchoolERP");

        result.ProposedAction
            .Should()
            .BeNull();
    }

    [Fact]
    public void Should_Reject_Action_Without_Proposal()
    {
        const string json =
            """
            {
              "kind": "Action",
              "message": "Fetching student."
            }
            """;

        var act = () =>
            AiSemanticResponseParser.Parse(json);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(
                "Action response must contain a proposed action.");
    }

    [Fact]
    public void Should_Reject_Message_With_Action()
    {
        const string json =
            """
            {
              "kind": "Message",
              "message": "Hello",
              "proposedAction": {
                "actionName": "GetStudentById",
                "version": 1
              }
            }
            """;

        var act = () =>
            AiSemanticResponseParser.Parse(json);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage(
                "Message response cannot contain a proposed action.");
    }
}