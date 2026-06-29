namespace ButterMorph.StudioPlayground.Models;

/// <summary>
/// Represents a request to execute one mapping.
/// </summary>
internal sealed class StudioExecutionRequest
{
    /// <summary>
    /// Gets or sets source JSON payloads by alias.
    /// </summary>
    public Dictionary<string, string> Sources { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
