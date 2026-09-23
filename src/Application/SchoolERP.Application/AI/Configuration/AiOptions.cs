namespace SchoolERP.Application.AI.Configuration;

public sealed class AiOptions
{
	public const string SectionName = "AI";

	public bool Enabled { get; init; }

	public string? DefaultProvider { get; init; }

	public string? DefaultModel { get; init; }

	public string? BaseUrl { get; init; }

	public string? ApiKey { get; init; }
}