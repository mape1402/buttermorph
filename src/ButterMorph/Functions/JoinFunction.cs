namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Joins scalar collection values into text.
/// </summary>
public sealed class JoinFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Joins scalar collection values into text.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "join", 2, 2);
        string separator = _tools.Text(context.Arguments[1]);
        IReadOnlyCollection<IScalarValue> values = _tools.ScalarValues(context.Arguments[0]);
        return _tools.StringResult(string.Join(separator, values.Select(_tools.Text)));
    }
}
