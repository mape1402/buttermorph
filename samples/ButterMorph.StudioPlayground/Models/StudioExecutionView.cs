namespace ButterMorph.StudioPlayground.Models;

/// <summary>
/// Represents a mapping execution result for the Studio UI.
/// </summary>
internal sealed class StudioExecutionView
{
    /// <summary>
    /// Gets or sets a value indicating whether execution succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the generated output JSON.
    /// </summary>
    public string OutputJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets execution diagnostics.
    /// </summary>
    public IReadOnlyCollection<string> Diagnostics { get; set; } = [];
}
