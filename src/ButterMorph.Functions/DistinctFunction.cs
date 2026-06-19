namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Removes duplicate scalar collection values.
/// </summary>
public sealed class DistinctFunction : IFunction
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
        _tools.Require(context, "distinct", 1, 2);
        List<IScalarValue> values = [];
        HashSet<string> seen = new(StringComparer.Ordinal);

        foreach (IScalarValue value in _tools.ScalarValues(context.Arguments[0]))
        {
            string text = _tools.Text(value);

            if (seen.Add(text))
            {
                values.Add(value);
            }
        }

        return _tools.ScalarCollectionResult(values);
    }
}
