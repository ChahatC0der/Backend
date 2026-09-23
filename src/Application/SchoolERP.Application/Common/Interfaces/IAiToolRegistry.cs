using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.Application.Common.Interfaces;

public interface IAiToolRegistry
{
    void Register(ToolDefinition tool);

    ToolDefinition? Get(
        string name,
        int version);

    IReadOnlyCollection<ToolDefinition> GetAll();
}