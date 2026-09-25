namespace ButterMorph.Abstractions;

/// <summary>
/// Represents one boolean validation assertion.
/// </summary>
public interface IValidationAssertion : IValidationStatement
{
    /// <summary>
    /// Gets the expression that must evaluate to true.
    /// </summary>
    ITransformationExpression Expression { get; }

    /// <summary>
    /// Gets the diagnostic message used when the assertion fails.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets the diagnostic path associated with the assertion.
    /// </summary>
    string Path { get; }
}
