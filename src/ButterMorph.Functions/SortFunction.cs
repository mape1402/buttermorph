namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Sorts scalar collection values.
/// </summary>
public sealed class SortFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Sorts scalar collection values.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "sort", 1, 2);
        List<IScalarValue> values = _tools.ScalarValues(context.Arguments[0]).ToList();
        bool descending = false;

        if (context.Arguments.Count > 1)
        {
            descending = string.Equals(_tools.Text(context.Arguments[1]), "desc", StringComparison.OrdinalIgnoreCase);
        }

        List<IScalarValue> sorted = values.OrderBy(_tools.Text, StringComparer.Ordinal).ToList();

        if (descending)
        {
            sorted.Reverse();
        }

        return _tools.ScalarCollectionResult(sorted);
    }
}
