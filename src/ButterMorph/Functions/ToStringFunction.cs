namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Converts a value to text.
/// </summary>
public sealed class ToStringFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Converts a value to text.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "toString", 1, 1);
        return _tools.StringResult(_tools.Text(context.Arguments[0]));
    }
}
