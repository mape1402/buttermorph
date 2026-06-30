namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Selects an evaluated value by matching a case value.
/// </summary>
public sealed class SwitchFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Selects an evaluated value by matching a case value.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "switch", 3, 17);
        IFunctionArgument value = _tools.Argument(context, "switch", 0);
        int index = 1;

        while (index + 1 < context.Arguments.Count)
        {
            if (_tools.Same(value, context.Arguments[index]))
            {
                return _tools.CloneArgument(context.Arguments[index + 1]);
            }

            index += 2;
        }

        if (index < context.Arguments.Count)
        {
            return _tools.CloneArgument(context.Arguments[index]);
        }

        return _tools.NullResult();
    }
}
