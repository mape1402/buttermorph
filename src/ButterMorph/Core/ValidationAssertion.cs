namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents one boolean validation assertion.
/// </summary>
public sealed class ValidationAssertion : IValidationAssertion
{
    /// <summary>
    /// Gets or sets the expression that must evaluate to true.
    /// </summary>
    public ITransformationExpression Expression { get; set; }

    /// <summary>
    /// Gets or sets the diagnostic message used when the assertion fails.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the diagnostic path associated with the assertion.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}
