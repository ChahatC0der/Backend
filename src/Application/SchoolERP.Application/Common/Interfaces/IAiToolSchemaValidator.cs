using System.Text.Json;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.Application.Common.Interfaces;

public interface IAiToolSchemaValidator
{
    AiToolSchemaValidationResult Validate(
        ToolDefinition tool,
        IReadOnlyDictionary<string, JsonElement> arguments);
}