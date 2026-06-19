namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Selects between two evaluated values using a condition.
/// </summary>
public sealed class IfFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Selects between two evaluated values using a condition.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "if", 3, 3);

        if (_tools.Truthy(_tools.Argument(context, "if", 0)))
        {
            return _tools.CloneArgument(_tools.Argument(context, "if", 1));
        }

        return _tools.CloneArgument(_tools.Argument(context, "if", 2));
    }
}
