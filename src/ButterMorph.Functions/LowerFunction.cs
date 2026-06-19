namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Converts text to lower case.
/// </summary>
public sealed class LowerFunction : IFunction
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
        _tools.Require(context, "lower", 1, 1);
        return _tools.StringResult(_tools.Text(_tools.Argument(context, "lower", 0)).ToLowerInvariant());
    }
}
