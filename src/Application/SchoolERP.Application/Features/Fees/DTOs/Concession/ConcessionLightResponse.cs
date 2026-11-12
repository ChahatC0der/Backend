// namespace line apni existing file wali hi rakhna
public sealed class ConcessionLightResponse
{
    public long Id { get; init; }        // Id ka type wahi jo purani DTO me tha (Guid/int/long)
    public string Name { get; init; } = string.Empty;

    // agar Concession entity me "Code" field hai to ye uncomment kar dena:
    // public string Code { get; init; } = string.Empty;
}