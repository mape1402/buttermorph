namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Describes a DSL function registration.
/// </summary>
public sealed class FunctionDescriptor : IFunctionDescriptor
{
    /// <summary>
    /// Gets or sets the unique function key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the function description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the produced value kind.
    /// </summary>
    public FunctionValueKind ValueKind { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the function is required by the catalog.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the function parameters.
    /// </summary>
    public IReadOnlyCollection<IFunctionParameterDescriptor> Parameters { get; set; } = [];

    /// <summary>
    /// Gets or sets UI and tooling metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
