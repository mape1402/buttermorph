namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Throws when a condition is not truthy.
/// </summary>
public sealed class AssertFunction : IFunction
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
        _tools.Require(context, "assert", 1, 2);

        if (_tools.Truthy(_tools.Argument(context, "assert", 0)))
        {
            return _tools.BooleanResult(true);
        }

        string message = "Assertion failed.";

        if (context.Arguments.Count > 1)
        {
            message = _tools.Text(_tools.Argument(context, "assert", 1));
        }

        throw new InvalidOperationException(message);
    }
}
