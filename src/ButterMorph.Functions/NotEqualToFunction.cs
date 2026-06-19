namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Checks whether two values are different.
/// </summary>
public sealed class NotEqualToFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Checks whether two values are different.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "neq", 2, 2);
        return _tools.BooleanResult(!_tools.Same(_tools.Argument(context, "neq", 0), _tools.Argument(context, "neq", 1)));
    }
}
