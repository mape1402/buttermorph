namespace ButterMorph.Functions;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Zips two scalar collections into node pairs.
/// </summary>
public sealed class ZipFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Zips two scalar collections into node pairs.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "zip", 2, 2);
        List<IScalarValue> left = _tools.ScalarValues(context.Arguments[0]).ToList();
        List<IScalarValue> right = _tools.ScalarValues(context.Arguments[1]).ToList();
        List<IStructureNode> nodes = [];
        int count = Math.Min(left.Count, right.Count);

        for (int index = 0; index < count; index++)
        {
            nodes.Add(new StructureNode
            {
                Name = index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Kind = StructureNodeKind.Object,
                Children =
                [
                    new ScalarStructureNode
                    {
                        Name = "left",
                        Value = _tools.CloneScalar(left[index])
                    },
                    new ScalarStructureNode
                    {
                        Name = "right",
                        Value = _tools.CloneScalar(right[index])
                    }
                ]
            });
        }

        return _tools.NodeCollectionResult(nodes);
    }
}
