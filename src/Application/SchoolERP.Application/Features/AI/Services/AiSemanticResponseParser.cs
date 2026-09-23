using System.Text.Json;
using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Features.AI.Services;

public static class AiSemanticResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static AiSemanticResponse Parse(
        string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(
                "AI returned an empty response.");
        }

        var response =
            JsonSerializer.Deserialize<AiSemanticResponse>(
                content,
                JsonOptions);

        if (response is null)
        {
            throw new InvalidOperationException(
                "AI returned an invalid semantic response.");
        }

        Validate(response);

        return response;
    }

    private static void Validate(
        AiSemanticResponse response)
    {
        switch (response.Kind)
        {
            case AiResponseKind.Message:

                if (response.ProposedAction is not null)
                {
                    throw new InvalidOperationException(
                        "Message response cannot contain a proposed action.");
                }

                break;

            case AiResponseKind.Action:

                if (response.ProposedAction is null)
                {
                    throw new InvalidOperationException(
                        "Action response must contain a proposed action.");
                }

                if (string.IsNullOrWhiteSpace(
                        response.ProposedAction.ActionName))
                {
                    throw new InvalidOperationException(
                        "Action name is required.");
                }

                if (response.ProposedAction.Version <= 0)
                {
                    throw new InvalidOperationException(
                        "Action version must be greater than zero.");
                }

                break;

            default:

                throw new InvalidOperationException(
                    "Unsupported AI response kind.");
        }
    }
}