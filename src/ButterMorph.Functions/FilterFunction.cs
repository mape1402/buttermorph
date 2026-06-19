namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Filters values using evaluated boolean masks.
/// </summary>
public sealed class FilterFunction : IFunction
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
        _tools.Require(context, "filter", 2, 2);
        List<IScalarValue> values = _tools.ScalarValues(context.Arguments[0]).ToList();
        List<IScalarValue> masks = _tools.ScalarValues(context.Arguments[1]).ToList();
        List<IScalarValue> filtered = [];

        if (masks.Count == 1)
        {
            if (_tools.Truthy(masks[0]))
            {
                return _tools.ScalarCollectionResult(values);
            }

            return _tools.ScalarCollectionResult(filtered);
        }

        for (int index = 0; index < values.Count && index < masks.Count; index++)
        {
            if (_tools.Truthy(masks[index]))
            {
                filtered.Add(values[index]);
            }
        }

        return _tools.ScalarCollectionResult(filtered);
    }
}
