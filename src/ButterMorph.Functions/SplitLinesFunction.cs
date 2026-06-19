namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Splits text into lines.
/// </summary>
public sealed class SplitLinesFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Splits text into lines.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "splitLines", 1, 1);
        string text = _tools.Text(context.Arguments[0]);

        if (string.IsNullOrEmpty(text))
        {
            return _tools.ScalarCollectionResult([]);
        }

        List<IScalarValue> values = [];

        foreach (string value in text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal).Split('\n'))
        {
            values.Add(_tools.StringValue(value));
        }

        return _tools.ScalarCollectionResult(values);
    }
}
