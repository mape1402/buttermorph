namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Counts items in a value.
/// </summary>
public sealed class CountFunction : IFunction
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
        _tools.Require(context, "count", 1, 1);

        if (context.Arguments[0] is IScalarCollectionFunctionArgument scalarCollectionArgument)
        {
            return _tools.NumberResult(scalarCollectionArgument.Values.Count);
        }

        if (context.Arguments[0] is IStructureNodeCollectionFunctionArgument nodeCollectionArgument)
        {
            return _tools.NumberResult(nodeCollectionArgument.Nodes.Count);
        }

        return _tools.NumberResult(1d);
    }
}
