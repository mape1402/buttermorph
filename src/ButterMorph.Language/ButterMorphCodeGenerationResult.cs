namespace ButterMorph.Language;

/// <summary>
/// Describes generated language code and diagnostics.
/// </summary>
public sealed class ButterMorphCodeGenerationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether code generation succeeded without diagnostics.
    /// </summary>
    public bool Succeeded { get; set; } = true;

    /// <summary>
    /// Gets or sets the generated code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets generation diagnostics.
    /// </summary>
    public IReadOnlyCollection<string> Diagnostics { get; set; } = [];
}
