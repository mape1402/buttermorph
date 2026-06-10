namespace ButterMorph.Functions;

/// <summary>
/// Describes a DSL function registration.
/// </summary>
public sealed class FunctionDescriptor
{
    /// <summary>
    /// Gets or sets the unique function key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the function description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
