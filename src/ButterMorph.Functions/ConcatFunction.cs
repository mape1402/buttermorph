namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Concatenates scalar values.
/// </summary>
public sealed class ConcatFunction : IFunction
{
    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "concat", 0, 16);
        return _tools.StringResult(string.Concat(context.Arguments.Select(_tools.Text)));
    }
}
