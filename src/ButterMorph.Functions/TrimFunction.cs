namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Trims surrounding text whitespace.
/// </summary>
public sealed class TrimFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Trims surrounding text whitespace.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "trim", 1, 1);
        return _tools.StringResult(_tools.Text(_tools.Argument(context, "trim", 0)).Trim());
    }
}
