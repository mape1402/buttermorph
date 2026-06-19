namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Reduces a scalar collection to one scalar value.
/// </summary>
public sealed class ReduceFunction : IFunction
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
        _tools.Require(context, "reduce", 1, 2);
        List<IScalarValue> values = _tools.ScalarValues(context.Arguments[0]).ToList();

        if (values.Count == 0)
        {
            if (context.Arguments.Count > 1)
            {
                return _tools.CloneArgument(context.Arguments[1]);
            }

            return _tools.NullResult();
        }

        bool allNumeric = true;
        double total = 0d;

        if (context.Arguments.Count > 1)
        {
            total = _tools.Number(context.Arguments[1]);
        }

        foreach (IScalarValue value in values)
        {
            if (double.TryParse(value.RawValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double number))
            {
                total += number;
            }
            else
            {
                allNumeric = false;
            }
        }

        if (allNumeric)
        {
            return _tools.NumberResult(total);
        }

        string seed = string.Empty;

        if (context.Arguments.Count > 1)
        {
            seed = _tools.Text(context.Arguments[1]);
        }

        return _tools.StringResult(seed + string.Concat(values.Select(_tools.Text)));
    }
}
