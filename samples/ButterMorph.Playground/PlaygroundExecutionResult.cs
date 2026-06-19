/// <summary>
/// Represents a playground transformation execution result.
/// </summary>
internal sealed class PlaygroundExecutionResult
{
    /// <summary>
    /// Gets or sets the scenario context key.
    /// </summary>
    public string ContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether execution succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the execution timestamp.
    /// </summary>
    public string ExecutedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mapping count.
    /// </summary>
    public int MappingCount { get; set; }

    /// <summary>
    /// Gets or sets the source JSON content by source key.
    /// </summary>
    public IReadOnlyDictionary<string, string> Sources { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the generated output JSON content.
    /// </summary>
    public string OutputJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets execution diagnostics.
    /// </summary>
    public IReadOnlyCollection<string> Diagnostics { get; set; } = [];
}
