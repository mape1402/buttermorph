namespace ButterMorph.Functions;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Groups scalar values by evaluated scalar keys.
/// </summary>
public sealed class GroupByFunction : IFunction
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
        _tools.Require(context, "groupBy", 2, 2);
        List<IScalarValue> values = _tools.ScalarValues(context.Arguments[0]).ToList();
        List<IScalarValue> keys = _tools.ScalarValues(context.Arguments[1]).ToList();
        Dictionary<string, List<IStructureNode>> groups = new(StringComparer.Ordinal);

        for (int index = 0; index < values.Count; index++)
        {
            string key = string.Empty;

            if (keys.Count == 1)
            {
                key = _tools.Text(keys[0]);
            }
            else if (index < keys.Count)
            {
                key = _tools.Text(keys[index]);
            }

            if (!groups.TryGetValue(key, out List<IStructureNode> children))
            {
                children = [];
                groups[key] = children;
            }

            children.Add(new ScalarStructureNode
            {
                Name = children.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Value = _tools.CloneScalar(values[index])
            });
        }

        List<IStructureNode> groupNodes = [];

        foreach (KeyValuePair<string, List<IStructureNode>> group in groups)
        {
            groupNodes.Add(new StructureNode
            {
                Name = group.Key,
                Kind = StructureNodeKind.Array,
                Children = group.Value
            });
        }

        return new StructureNodeFunctionResult
        {
            Node = new StructureNode
            {
                Name = "$groups",
                Kind = StructureNodeKind.Object,
                Children = groupNodes
            }
        };
    }
}
