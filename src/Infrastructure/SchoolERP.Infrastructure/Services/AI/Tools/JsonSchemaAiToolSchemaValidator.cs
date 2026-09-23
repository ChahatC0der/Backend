using System.Text.Json;
using Json.Schema;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.Infrastructure.Services.AI.Tools;

public sealed class JsonSchemaAiToolSchemaValidator
    : IAiToolSchemaValidator
{
    public AiToolSchemaValidationResult Validate(
        ToolDefinition tool,
        IReadOnlyDictionary<string, JsonElement> arguments)
    {
        ArgumentNullException.ThrowIfNull(tool);
        ArgumentNullException.ThrowIfNull(arguments);

        JsonSchema schema;

        try
        {
            schema = JsonSchema.FromText(
                tool.InputSchema.ToJsonString());
        }
        catch (Exception ex)
        {
            return AiToolSchemaValidationResult.Failure(
            [
                $"Tool '{tool.Name}' contains an invalid input schema: {ex.Message}"
            ]);
        }

        var instance =
            JsonSerializer.SerializeToElement(arguments);

        var evaluationOptions = new EvaluationOptions
        {
            OutputFormat = OutputFormat.List
        };

        var result = schema.Evaluate(
            instance,
            evaluationOptions);

        if (result.IsValid)
        {
            return AiToolSchemaValidationResult.Success();
        }

        var errors = new List<string>();

        CollectErrors(
            result,
            errors);

        return AiToolSchemaValidationResult.Failure(
            errors);
    }

    private static void CollectErrors(
        EvaluationResults result,
        ICollection<string> errors)
    {
        foreach (var error in result.Errors)
        {
            var location =
                result.InstanceLocation.ToString();

            errors.Add(
                string.IsNullOrWhiteSpace(location)
                    ? error.Value
                    : $"{location}: {error.Value}");
        }

        foreach (var detail in result.Details)
        {
            CollectErrors(
                detail,
                errors);
        }
    }
}