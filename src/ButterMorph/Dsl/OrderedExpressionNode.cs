namespace ButterMorph.Dsl;

// Represents an ordered expression in the internal syntax tree.
internal sealed class OrderedExpressionNode : AstNode
{
    internal List<AstNode> Items { get; } = [];
}
