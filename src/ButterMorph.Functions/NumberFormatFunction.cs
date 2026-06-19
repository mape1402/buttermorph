namespace ButterMorph.Functions;

using System.Globalization;
using ButterMorph.Abstractions;

/// <summary>
/// Formats a number using invariant culture.
/// </summary>
public sealed class NumberFormatFunction : IFunction
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
        _tools.Require(context, "numberFormat", 2, 2);
        return _tools.StringResult(_tools.Number(context.Arguments[0]).ToString(_tools.Text(context.Arguments[1]), CultureInfo.InvariantCulture));
    }
}
