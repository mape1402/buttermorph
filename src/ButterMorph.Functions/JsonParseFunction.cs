namespace ButterMorph.Functions;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Parses JSON text into a structure node.
/// </summary>
public sealed class JsonParseFunction : IFunction
{
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
        _tools.Require(context, "jsonParse", 1, 1);
        return new StructureNodeFunctionResult
        {
            Node = _json.ReadNode(_tools.Text(context.Arguments[0]))
        };
    }
}
