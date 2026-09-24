namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents an editable validation assertion row.
/// </summary>
public sealed class ValidationAssertionDisplayModel
{
    /// <summary>
    /// Gets or sets the assertion expression as DSL text.
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user-facing validation message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the diagnostic path.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}
