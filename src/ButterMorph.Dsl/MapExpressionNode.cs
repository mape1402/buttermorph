namespace ButterMorph.Dsl;

// Represents an inline map-shaped expression in the internal syntax tree.
internal sealed class MapExpressionNode : AstNode
{
    internal List<PropertyExpressionNode> Properties { get; } = [];
}
