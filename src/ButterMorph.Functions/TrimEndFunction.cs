namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Removes trailing whitespace from text.
/// </summary>
public sealed class TrimEndFunction : IFunction
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
        _tools.Require(context, "trimEnd", 1, 1);
        return _tools.StringResult(_tools.Text(context.Arguments[0]).TrimEnd());
    }
}
