namespace ButterMorph.Functions;

using System.Globalization;
using ButterMorph.Abstractions;

/// <summary>
/// Parses a date and returns an ISO value.
/// </summary>
public sealed class ParseDateFunction : IFunction
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
        _tools.Require(context, "parseDate", 1, 2);

        if (context.Arguments.Count == 2)
        {
            return _dates.DateResult(_dates.ParseExact(context.Arguments[0], _tools.Text(context.Arguments[1])));
        }

        return _dates.DateResult(_dates.Parse(context.Arguments[0]));
    }
}
