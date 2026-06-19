namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Serializes a function argument into JSON text.
/// </summary>
public sealed class JsonStringifyFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Serializes a function argument into JSON text.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    // Shared JSON conversion helpers for this function.
    private readonly JsonFunctionConverter _json = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "jsonStringify", 1, 1);
        IFunctionArgument argument = context.Arguments[0];

        if (argument is IStructureNodeFunctionArgument nodeArgument)
        {
            return _tools.StringResult(_json.WriteNode(nodeArgument.Node));
        }

        if (argument is IStructureNodeCollectionFunctionArgument nodeCollectionArgument)
        {
            string text = "[" + string.Join(",", nodeCollectionArgument.Nodes.Select(_json.WriteNode)) + "]";
            return _tools.StringResult(text);
        }

        if (argument is IScalarFunctionArgument scalarArgument)
        {
            return _tools.StringResult(_json.WriteScalar(scalarArgument.Value));
        }

        if (argument is IScalarCollectionFunctionArgument scalarCollectionArgument)
        {
            string text = "[" + string.Join(",", scalarCollectionArgument.Values.Select(_json.WriteScalar)) + "]";
            return _tools.StringResult(text);
        }

        return _tools.StringResult(_tools.Text(argument));
    }
}
