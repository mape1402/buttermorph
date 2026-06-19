namespace ButterMorph.Functions;

using System.Globalization;
using ButterMorph.Abstractions;

/// <summary>
/// Formats a date using an invariant format string.
/// </summary>
public sealed class FormatDateFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Formats a date using an invariant format string.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    // Shared date parsing helpers for this function.
    private readonly DateFunctionTools _dates = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "formatDate", 2, 2);
        return _tools.StringResult(_dates.Parse(context.Arguments[0]).ToString(_tools.Text(context.Arguments[1]), CultureInfo.InvariantCulture));
    }
}
