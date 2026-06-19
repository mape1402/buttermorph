namespace ButterMorph.Functions;

using System.Globalization;
using ButterMorph.Abstractions;

/// <summary>
/// Returns date difference in minutes.
/// </summary>
public sealed class DiffMinutesFunction : IFunction
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
        _tools.Require(context, "diffMinutes", 2, 2);
        return _tools.NumberResult((_dates.Parse(context.Arguments[0]) - _dates.Parse(context.Arguments[1])).TotalMinutes);
    }
}
