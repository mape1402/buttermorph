namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a validation rule declaration.
/// </summary>
public interface IValidationRule
{
    /// <summary>
    /// Gets the graph path validated by the rule.
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Gets the registered rule handler key.
    /// </summary>
    string RuleKey { get; }

    /// <summary>
    /// Gets the expected scalar data type.
    /// </summary>
    string ExpectedDataType { get; }

    /// <summary>
    /// Gets the expected scalar value.
    /// </summary>
    string ExpectedValue { get; }
}
