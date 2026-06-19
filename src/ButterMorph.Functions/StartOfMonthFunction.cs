namespace ButterMorph.Functions;

using System.Globalization;
using ButterMorph.Abstractions;

/// <summary>
/// Returns the first moment of the month.
/// </summary>
public sealed class StartOfMonthFunction : IFunction
{
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
        _tools.Require(context, "startOfMonth", 1, 1);
        DateTimeOffset date = _dates.Parse(context.Arguments[0]);
        return _dates.DateResult(new DateTimeOffset(date.Year, date.Month, 1, 0, 0, 0, date.Offset));
    }
}
