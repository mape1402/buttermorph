namespace ButterMorph.Abstractions;

/// <summary>
/// Describes one function for design-time discovery.
/// </summary>
public interface IFunctionDescriptor
{
    /// <summary>
    /// Gets the unique function key.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the function description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the produced value kind.
    /// </summary>
    FunctionValueKind ValueKind { get; }

    /// <summary>
    /// Gets a value indicating whether the function is required by the catalog.
    /// </summary>
    bool IsRequired { get; }

    /// <summary>
    /// Gets the function parameters.
    /// </summary>
    IReadOnlyCollection<IFunctionParameterDescriptor> Parameters { get; }

    /// <summary>
    /// Gets UI and tooling metadata.
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }
}
