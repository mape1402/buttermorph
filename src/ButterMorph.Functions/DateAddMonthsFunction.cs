namespace ButterMorph.Functions;

using System.Globalization;
using ButterMorph.Abstractions;

/// <summary>
/// Adds months to a date.
/// </summary>
public sealed class DateAddMonthsFunction : IFunction
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
        _tools.Require(context, "dateAddMonths", 2, 2);
        return _dates.DateResult(_dates.Parse(context.Arguments[0]).AddMonths(Convert.ToInt32(_tools.Number(context.Arguments[1]))));
    }
}
