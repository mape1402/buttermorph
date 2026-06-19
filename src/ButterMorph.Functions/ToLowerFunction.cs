namespace ButterMorph.Functions;

using System.Globalization;
using ButterMorph.Abstractions;

/// <summary>
/// Converts text to lowercase.
/// </summary>
public sealed class ToLowerFunction : IFunction
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
        _tools.Require(context, "ToLower", 1, 1);
        return _tools.StringResult(_tools.Text(context.Arguments[0]).ToLower(CultureInfo.InvariantCulture));
    }
}
