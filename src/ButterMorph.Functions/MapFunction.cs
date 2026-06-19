namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns an already evaluated projected collection.
/// </summary>
public sealed class MapFunction : IFunction
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
        _tools.Require(context, "map", 1, 1);
        return _tools.ScalarCollectionResult(_tools.ScalarValues(context.Arguments[0]));
    }
}
