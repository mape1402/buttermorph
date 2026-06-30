namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that calls a registered function.
/// </summary>
public interface IFunctionCallExpression : ITransformationExpression
{
    /// <summary>
    /// Gets the registered function key.
    /// </summary>
    string FunctionKey { get; }

    /// <summary>
    /// Gets the function argument expressions.
    /// </summary>
    IReadOnlyCollection<ITransformationExpression> Arguments { get; }
}
