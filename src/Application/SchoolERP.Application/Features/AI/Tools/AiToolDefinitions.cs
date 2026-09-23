using System.Text.Json.Nodes;

namespace SchoolERP.Application.Features.AI.Tools;

public static class AiToolDefinitions
{
    public static IReadOnlyCollection<ToolDefinition> GetAll()
    {
        return
        [
            new ToolDefinition
            {
                Name =
                    "GetStudentByAdmissionNumber",

                Description =
                    "Retrieves a student using their admission number.",

                Version = 1,

                InputSchema = new JsonObject
                {
                    ["type"] = "object",

                    ["properties"] = new JsonObject
                    {
                        ["admissionNumber"] = new JsonObject
                        {
                            ["type"] = "string"
                        }
                    },

                    ["required"] = new JsonArray
                    {
                        "admissionNumber"
                    },

                    ["additionalProperties"] = false
                },

                // Authorization metadata will be finalized
                // when AI is connected to the existing RBAC engine.
                RequiredPermission = null,

                // Scope enforcement will be finalized
                // in the AI authorization/data-scope phase.
                DataScope = AiDataScope.None,

                RiskLevel =
                    AiRiskLevel.Low,

                ConfirmationPolicy =
                    AiConfirmationPolicy.None,

                AuditPolicy =
                    AiAuditPolicy.Required
            }
        ];
    }
}