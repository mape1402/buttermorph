namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Describes one function parameter for design-time discovery.
/// </summary>
public sealed class FunctionParameterDescriptor : IFunctionParameterDescriptor
{
    /// <summary>
    /// Gets or sets the parameter key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parameter description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the accepted value kind.
    /// </summary>
    public FunctionValueKind ValueKind { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the parameter is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets UI and tooling metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
