namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Returns the length of text or the count of a collection.
/// </summary>
public sealed class LengthFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Returns the length of text or the count of a collection.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "length", 1, 1);
        IFunctionArgument argument = context.Arguments[0];

        if (argument is IScalarCollectionFunctionArgument scalarCollection)
        {
            return _tools.NumberResult(scalarCollection.Values.Count);
        }

        if (argument is IStructureNodeCollectionFunctionArgument nodeCollection)
        {
            return _tools.NumberResult(nodeCollection.Nodes.Count);
        }

        if (argument is IStructureNodeFunctionArgument nodeArgument)
        {
            return _tools.NumberResult(nodeArgument.Node.Children.Count);
        }

        return _tools.NumberResult(_tools.Text(argument).Length);
    }
}
