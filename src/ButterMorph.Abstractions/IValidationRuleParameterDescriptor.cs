namespace ButterMorph.Abstractions;

/// <summary>
/// Describes one validation rule parameter for design-time discovery.
/// </summary>
public interface IValidationRuleParameterDescriptor
{
    /// <summary>
    /// Gets the parameter key.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the parameter description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the accepted value kind.
    /// </summary>
    FunctionValueKind ValueKind { get; }

    /// <summary>
    /// Gets a value indicating whether the parameter is required.
    /// </summary>
    bool IsRequired { get; }

    /// <summary>
    /// Gets UI and tooling metadata.
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }
}
