using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.Infrastructure.Services.AI.Tools;

public sealed class InMemoryAiToolRegistry
    : IAiToolRegistry
{
    private readonly Dictionary<
        string,
        ToolDefinition> _tools =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(ToolDefinition tool)
    {
        ArgumentNullException.ThrowIfNull(tool);

        if (string.IsNullOrWhiteSpace(tool.Name))
        {
            throw new ArgumentException(
                "Tool name is required.",
                nameof(tool));
        }

        if (tool.Version <= 0)
        {
            throw new ArgumentException(
                "Tool version must be greater than zero.",
                nameof(tool));
        }

        var key = BuildKey(
            tool.Name,
            tool.Version);

        if (_tools.ContainsKey(key))
        {
            throw new InvalidOperationException(
                $"AI tool '{tool.Name}' version '{tool.Version}' is already registered.");
        }

        _tools.Add(key, tool);
    }

    public ToolDefinition? Get(
        string name,
        int version)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (version <= 0)
        {
            return null;
        }

        var key = BuildKey(
            name,
            version);

        return _tools.GetValueOrDefault(key);
    }

    public IReadOnlyCollection<ToolDefinition> GetAll()
    {
        return _tools.Values.ToArray();
    }

    private static string BuildKey(
        string name,
        int version)
    {
        return $"{name.Trim()}:{version}";
    }
}