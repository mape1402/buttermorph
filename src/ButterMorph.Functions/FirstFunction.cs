namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns the first item from a collection.
/// </summary>
public sealed class FirstFunction : IFunction
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
        _tools.Require(context, "first", 1, 1);
        IReadOnlyCollection<IScalarValue> values = _tools.ScalarValues(context.Arguments[0]);

        foreach (IScalarValue value in values)
        {
            return _tools.ScalarResult(_tools.CloneScalar(value));
        }

        return _tools.NullResult();
    }
}
