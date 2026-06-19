namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Splits text using a literal separator.
/// </summary>
public sealed class SplitFunction : IFunction
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
        _tools.Require(context, "split", 2, 2);
        string text = _tools.Text(context.Arguments[0]);
        string separator = _tools.Text(context.Arguments[1]);

        if (string.IsNullOrEmpty(text))
        {
            return _tools.ScalarCollectionResult([]);
        }

        if (string.IsNullOrEmpty(separator))
        {
            throw new InvalidOperationException("Function 'split' expects a non-empty separator.");
        }

        List<IScalarValue> values = [];

        foreach (string value in text.Split([separator], StringSplitOptions.None))
        {
            values.Add(_tools.StringValue(value));
        }

        return _tools.ScalarCollectionResult(values);
    }
}
