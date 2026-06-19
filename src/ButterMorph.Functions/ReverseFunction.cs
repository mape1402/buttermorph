namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Reverses scalar collection values.
/// </summary>
public sealed class ReverseFunction : IFunction
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
        _tools.Require(context, "reverse", 1, 1);
        return _tools.ScalarCollectionResult(_tools.ScalarValues(context.Arguments[0]).Reverse().ToList());
    }
}
