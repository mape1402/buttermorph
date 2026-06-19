namespace ButterMorph.Functions;

using System.Globalization;
using ButterMorph.Abstractions;

/// <summary>
/// Returns the final moment of the month.
/// </summary>
public sealed class EndOfMonthFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Returns the final moment of the month.";

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
        _tools.Require(context, "endOfMonth", 1, 1);
        DateTimeOffset date = _dates.Parse(context.Arguments[0]);
        DateTimeOffset start = new(date.Year, date.Month, 1, 0, 0, 0, date.Offset);
        return _dates.DateResult(start.AddMonths(1).AddTicks(-1));
    }
}
