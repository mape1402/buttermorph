namespace ButterMorph.Functions;

using System.Globalization;
using ButterMorph.Abstractions;

/// <summary>
/// Capitalizes the first text character.
/// </summary>
public sealed class CapitalizeFunction : IFunction
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
        _tools.Require(context, "capitalize", 1, 1);
        string text = _tools.Text(context.Arguments[0]);

        if (string.IsNullOrEmpty(text))
        {
            return _tools.StringResult(text);
        }

        return _tools.StringResult(char.ToUpper(text[0], CultureInfo.InvariantCulture) + text[1..]);
    }
}
