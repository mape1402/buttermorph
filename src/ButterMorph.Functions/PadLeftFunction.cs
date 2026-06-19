namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Pads text on the left side.
/// </summary>
public sealed class PadLeftFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Pads text on the left side.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "padLeft", 3, 3);
        return _tools.StringResult(_tools.Text(context.Arguments[0]).PadLeft(Convert.ToInt32(_tools.Number(context.Arguments[1])), ResolvePadding(_tools.Text(context.Arguments[2]))));
    }

    // Resolves the padding character.
    private static char ResolvePadding(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return ' ';
        }

        return value[0];
    }
}
