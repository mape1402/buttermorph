namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Removes leading whitespace from text.
/// </summary>
public sealed class TrimStartFunction : IFunction
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
        _tools.Require(context, "trimStart", 1, 1);
        return _tools.StringResult(_tools.Text(context.Arguments[0]).TrimStart());
    }
}
