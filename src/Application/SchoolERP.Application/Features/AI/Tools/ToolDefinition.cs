using System.Text.Json.Nodes;

namespace SchoolERP.Application.Features.AI.Tools;

public sealed record ToolDefinition
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public int Version { get; init; } = 1;

    public required JsonObject InputSchema { get; init; } = new();

    public string? RequiredPermission { get; init; }

    public AiDataScope DataScope { get; init; }
        = AiDataScope.None;

    public AiRiskLevel RiskLevel { get; init; }
        = AiRiskLevel.Low;

    public AiConfirmationPolicy ConfirmationPolicy { get; init; }
        = AiConfirmationPolicy.None;

    public AiAuditPolicy AuditPolicy { get; init; }
        = AiAuditPolicy.Required;
}

public enum AiDataScope
{
    None,
    User,
    Branch,
    Tenant
}

public enum AiRiskLevel
{
    Low,
    Medium,
    High,
    Critical
}

public enum AiConfirmationPolicy
{
    None,
    Required
}

public enum AiAuditPolicy
{
    None,
    Required
}