namespace ButterMorph.Dsl;

// Stores target path segments while rendering nested target blocks.
internal sealed class DslTargetNode
{
    internal string Name { get; set; } = string.Empty;

    internal bool HasExpression { get; set; }

    internal string Expression { get; set; } = string.Empty;

    internal Dictionary<string, DslTargetNode> Children { get; } = new(StringComparer.Ordinal);
}
