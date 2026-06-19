namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns the last item from a collection.
/// </summary>
public sealed class LastFunction : IFunction
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
        _tools.Require(context, "last", 1, 1);
        IReadOnlyCollection<IScalarValue> values = _tools.ScalarValues(context.Arguments[0]);
        IScalarValue last = null;

        foreach (IScalarValue value in values)
        {
            last = value;
        }

        if (last == null)
        {
            return _tools.NullResult();
        }

        return _tools.ScalarResult(_tools.CloneScalar(last));
    }
}
