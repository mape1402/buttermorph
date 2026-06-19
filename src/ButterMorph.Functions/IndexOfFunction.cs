namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns the first index of a search value.
/// </summary>
public sealed class IndexOfFunction : IFunction
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
        _tools.Require(context, "indexOf", 2, 2);
        return _tools.NumberResult(_tools.Text(context.Arguments[0]).IndexOf(_tools.Text(context.Arguments[1]), StringComparison.Ordinal));
    }
}
