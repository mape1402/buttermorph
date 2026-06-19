namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Flattens scalar values into a scalar collection.
/// </summary>
public sealed class FlattenFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Flattens scalar values into a scalar collection.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "flatten", 1, 2);
        return _tools.ScalarCollectionResult(_tools.ScalarValues(context.Arguments[0]));
    }
}
