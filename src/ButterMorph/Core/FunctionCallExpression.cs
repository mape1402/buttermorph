namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a transformation expression that calls a registered function.
/// </summary>
public sealed class FunctionCallExpression : IFunctionCallExpression
{
    /// <summary>
    /// Gets the expression kind.
    /// </summary>
    public TransformationExpressionKind Kind => TransformationExpressionKind.FunctionCall;

    /// <summary>
    /// Gets or sets the registered function key.
    /// </summary>
    public string FunctionKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the function argument expressions.
    /// </summary>
    public IReadOnlyCollection<ITransformationExpression> Arguments { get; set; } = [];
}
