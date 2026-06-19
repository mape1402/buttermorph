namespace ButterMorph.Functions;

using System.Globalization;
using ButterMorph.Abstractions;

/// <summary>
/// Converts a date to a time zone.
/// </summary>
public sealed class ToTimeZoneFunction : IFunction
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
        _tools.Require(context, "toTimeZone", 2, 2);
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(_tools.Text(context.Arguments[1]));
        return _dates.DateResult(TimeZoneInfo.ConvertTime(_dates.Parse(context.Arguments[0]), timeZone));
    }
}
