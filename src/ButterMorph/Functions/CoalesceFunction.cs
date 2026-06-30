namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns the first non-null value.
/// </summary>
public sealed class CoalesceFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Returns the first non-null value.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "coalesce", 1, 16);

        foreach (IFunctionArgument argument in context.Arguments)
        {
            if (!_tools.IsNull(argument))
            {
                return _tools.CloneArgument(argument);
            }
        }

        return _tools.NullResult();
    }
}
