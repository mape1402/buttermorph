namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents a schema designer host save response.
/// </summary>
public sealed class SchemaDesignerHostSaveResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the host save completed.
    /// </summary>
    public bool HostSaveCompleted { get; set; }

    /// <summary>
    /// Gets or sets the saved context key.
    /// </summary>
    public string SavedContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the host message type.
    /// </summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user-facing save message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets detailed user-facing save messages.
    /// </summary>
    public IReadOnlyCollection<string> Details { get; set; } = [];

    /// <summary>
    /// Gets or sets the safe local return URL for non-opener host flows.
    /// </summary>
    public string SafeReturnUrl { get; set; } = string.Empty;
}
